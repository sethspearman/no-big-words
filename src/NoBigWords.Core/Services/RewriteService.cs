using System.Text;
using System.Text.RegularExpressions;
using NoBigWords.Core.Abstractions;
using NoBigWords.Core.Models;

namespace NoBigWords.Core.Services;

public sealed partial class RewriteService(
    IAllowedWordsProvider allowedWordsProvider,
    IReplacementDictionary replacementDictionary,
    IAiRewriteService aiRewriteService) : IRewriteService
{
    [GeneratedRegex("[A-Za-z]+(?:['’][A-Za-z]+)?|[^A-Za-z]+", RegexOptions.Compiled)]
    private static partial Regex TokenRegex();

    [GeneratedRegex("[A-Za-z]+(?:['’][A-Za-z]+)?", RegexOptions.Compiled)]
    private static partial Regex WordRegex();

    public async Task<RewriteResponse> RewriteAsync(RewriteRequest request, CancellationToken cancellationToken = default)
    {
        var text = request.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            return new RewriteResponse(
                text,
                string.Empty,
                [],
                [],
                new ValidationSummary(0, 0, 0, 100),
                UsedAi: false);
        }

        return request.Mode switch
        {
            RewriteMode.NoBigWords => RewriteNoBigWords(text),
            RewriteMode.ExplainLikeImTen => await RewriteExplainModeAsync(text, cancellationToken),
            _ => RewriteNoBigWords(text),
        };
    }

    private RewriteResponse RewriteNoBigWords(string text)
    {
        var replacements = new List<ReplacementResult>();
        var unknownWords = new List<UnknownWord>();
        var builder = new StringBuilder(text.Length + 64);

        foreach (Match match in TokenRegex().Matches(text))
        {
            var token = match.Value;

            if (!IsWord(token))
            {
                builder.Append(token);
                continue;
            }

            var normalized = Normalize(token);
            var outputStart = builder.Length;

            if (allowedWordsProvider.IsAllowed(normalized))
            {
                builder.Append(token);
                continue;
            }

            if (TryResolveReplacement(token, normalized, out var replacement, out var source, out var confidence))
            {
                builder.Append(replacement);
                replacements.Add(new ReplacementResult(
                    token,
                    replacement,
                    outputStart,
                    replacement.Length,
                    source,
                    confidence));
                continue;
            }

            builder.Append(token);
            unknownWords.Add(new UnknownWord(token, outputStart, token.Length));
        }

        var rewritten = builder.ToString();
        var validation = BuildValidationSummary(rewritten);

        return new RewriteResponse(
            text,
            rewritten,
            replacements,
            unknownWords,
            validation,
            UsedAi: false,
            Message: unknownWords.Count == 0 ? null : "Some words could not be simplified with the local dictionary yet.");
    }

    private async Task<RewriteResponse> RewriteExplainModeAsync(string text, CancellationToken cancellationToken)
    {
        var rewritten = await aiRewriteService.RewriteAsync(text, RewriteMode.ExplainLikeImTen, cancellationToken);
        var validation = BuildValidationSummary(rewritten);

        return new RewriteResponse(
            text,
            rewritten,
            [],
            [],
            validation,
            UsedAi: true,
            Message: "Explain Like I'm 10 mode is using the AI rewrite abstraction.");
    }

    private bool TryResolveReplacement(
        string originalToken,
        string normalized,
        out string replacement,
        out ReplacementSource source,
        out double confidence)
    {
        if (replacementDictionary.TryGetReplacement(normalized, out var direct) && ReplacementPhraseIsAllowed(direct.Replacement))
        {
            replacement = ApplyCasing(originalToken, direct.Replacement);
            source = ReplacementSource.LocalDictionary;
            confidence = direct.Confidence;
            return true;
        }

        foreach (var stem in GetCandidateStems(normalized))
        {
            if (allowedWordsProvider.IsAllowed(stem))
            {
                replacement = ApplyCasing(originalToken, stem);
                source = ReplacementSource.InflectionRule;
                confidence = 0.75;
                return true;
            }

            if (replacementDictionary.TryGetReplacement(stem, out var inferred) && ReplacementPhraseIsAllowed(inferred.Replacement))
            {
                replacement = ApplyCasing(originalToken, inferred.Replacement);
                source = ReplacementSource.InflectionRule;
                confidence = inferred.Confidence * 0.9;
                return true;
            }
        }

        replacement = string.Empty;
        source = ReplacementSource.AllowedAsIs;
        confidence = 0;
        return false;
    }

    private ValidationSummary BuildValidationSummary(string text)
    {
        var words = WordRegex().Matches(text).Select(match => Normalize(match.Value)).ToList();
        var totalWords = words.Count;

        if (totalWords == 0)
        {
            return new ValidationSummary(0, 0, 0, 100);
        }

        var allowedWords = words.Count(allowedWordsProvider.IsAllowed);
        var disallowedWords = totalWords - allowedWords;
        var allowedPercentage = Math.Round((double)allowedWords / totalWords * 100, 2);

        return new ValidationSummary(totalWords, allowedWords, disallowedWords, allowedPercentage);
    }

    private bool ReplacementPhraseIsAllowed(string replacement)
    {
        foreach (Match match in WordRegex().Matches(replacement))
        {
            if (!allowedWordsProvider.IsAllowed(Normalize(match.Value)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsWord(string token) => char.IsLetter(token[0]);

    private static string Normalize(string value) => value.Trim().ToLowerInvariant().Replace('’', '\'');

    private static IEnumerable<string> GetCandidateStems(string word)
    {
        if (word.EndsWith("ies", StringComparison.Ordinal) && word.Length > 3)
        {
            yield return string.Concat(word.AsSpan(0, word.Length - 3), "y");
        }

        if (word.EndsWith("ing", StringComparison.Ordinal) && word.Length > 4)
        {
            yield return word[..^3];
        }

        if (word.EndsWith("ed", StringComparison.Ordinal) && word.Length > 3)
        {
            yield return word[..^2];
        }

        if (word.EndsWith("es", StringComparison.Ordinal) && word.Length > 3)
        {
            yield return word[..^2];
        }

        if (word.EndsWith("s", StringComparison.Ordinal) && word.Length > 2)
        {
            yield return word[..^1];
        }
    }

    private static string ApplyCasing(string original, string replacement)
    {
        if (string.IsNullOrWhiteSpace(replacement))
        {
            return replacement;
        }

        if (original.All(char.IsUpper))
        {
            return replacement.ToUpperInvariant();
        }

        if (char.IsUpper(original[0]))
        {
            var chars = replacement.ToCharArray();
            chars[0] = char.ToUpperInvariant(chars[0]);
            return new string(chars);
        }

        return replacement;
    }
}
