namespace NoBigWords.Core.Models;

public sealed record RewriteRequest(
    string Text,
    RewriteMode Mode);

public sealed record RewriteResponse(
    string OriginalText,
    string RewrittenText,
    IReadOnlyList<ReplacementResult> Replacements,
    IReadOnlyList<UnknownWord> UnknownWords,
    ValidationSummary Validation,
    bool UsedAi,
    string? Message = null);

public sealed record ReplacementResult(
    string Original,
    string Replacement,
    int StartIndex,
    int Length,
    ReplacementSource Source,
    double Confidence);

public enum ReplacementSource
{
    AllowedAsIs,
    LocalDictionary,
    InflectionRule,
    OpenAiFallback,
}

public sealed record UnknownWord(
    string Word,
    int StartIndex,
    int Length);

public sealed record ValidationSummary(
    int TotalWords,
    int AllowedWords,
    int DisallowedWords,
    double AllowedPercentage);

public sealed record ReplacementEntry(
    string Replacement,
    double Confidence,
    string? Notes = null);
