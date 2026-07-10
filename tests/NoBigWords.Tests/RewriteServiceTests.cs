using NoBigWords.Core.Abstractions;
using NoBigWords.Core.Models;
using NoBigWords.Core.Services;

namespace NoBigWords.Tests;

public sealed class RewriteServiceTests
{
    [Fact]
    public async Task RewriteAsync_KeepsAllowedWordsAsIs()
    {
        var service = CreateService(
            allowedWords: ["hello", "world"],
            replacements: new Dictionary<string, ReplacementEntry>());

        var response = await service.RewriteAsync(new RewriteRequest("hello world", RewriteMode.NoBigWords));

        Assert.Equal("hello world", response.RewrittenText);
        Assert.Empty(response.UnknownWords);
        Assert.Equal(100, response.Validation.AllowedPercentage);
    }

    [Fact]
    public async Task RewriteAsync_UsesLocalDictionaryWhenAvailable()
    {
        var service = CreateService(
            allowedWords: ["space", "person"],
            replacements: new Dictionary<string, ReplacementEntry>
            {
                ["astronaut"] = new("space person", 0.9),
            });

        var response = await service.RewriteAsync(new RewriteRequest("astronaut", RewriteMode.NoBigWords));

        Assert.Equal("space person", response.RewrittenText);
        Assert.Single(response.Replacements);
        Assert.Equal(ReplacementSource.LocalDictionary, response.Replacements[0].Source);
    }

    [Fact]
    public async Task RewriteAsync_ReportsUnknownWordsWhenNoReplacementExists()
    {
        var service = CreateService(
            allowedWords: ["hello"],
            replacements: new Dictionary<string, ReplacementEntry>());

        var response = await service.RewriteAsync(new RewriteRequest("galaxy", RewriteMode.NoBigWords));

        Assert.Single(response.UnknownWords);
        Assert.Equal("galaxy", response.UnknownWords[0].Word);
        Assert.Equal(1, response.Validation.DisallowedWords);
    }

    [Fact]
    public async Task RewriteAsync_PreservesSimplePunctuationAroundReplacement()
    {
        var service = CreateService(
            allowedWords: ["space", "person"],
            replacements: new Dictionary<string, ReplacementEntry>
            {
                ["astronaut"] = new("space person", 0.9),
            });

        var response = await service.RewriteAsync(new RewriteRequest("astronaut!", RewriteMode.NoBigWords));

        Assert.Equal("space person!", response.RewrittenText);
    }

    [Fact]
    public async Task RewriteAsync_UsesAiPathForExplainMode()
    {
        var service = new RewriteService(
            new FakeAllowedWordsProvider(["hello"]),
            new FakeReplacementDictionary(new Dictionary<string, ReplacementEntry>()),
            new FakeAiRewriteService("simple words here"));

        var response = await service.RewriteAsync(new RewriteRequest("Complicated source text", RewriteMode.ExplainLikeImTen));

        Assert.Equal("simple words here", response.RewrittenText);
        Assert.True(response.UsedAi);
    }

    private static RewriteService CreateService(
        IReadOnlyCollection<string> allowedWords,
        IDictionary<string, ReplacementEntry> replacements)
    {
        return new RewriteService(
            new FakeAllowedWordsProvider(allowedWords),
            new FakeReplacementDictionary(replacements),
            new FakeAiRewriteService());
    }

    private sealed class FakeAllowedWordsProvider(IReadOnlyCollection<string> words) : IAllowedWordsProvider
    {
        private readonly HashSet<string> _words = words.ToHashSet(StringComparer.Ordinal);

        public bool IsAllowed(string word) => _words.Contains(word);
    }

    private sealed class FakeReplacementDictionary(IDictionary<string, ReplacementEntry> entries) : IReplacementDictionary
    {
        private readonly Dictionary<string, ReplacementEntry> _entries = new(entries, StringComparer.Ordinal);

        public bool TryGetReplacement(string word, out ReplacementEntry replacement) => _entries.TryGetValue(word, out replacement!);
    }

    private sealed class FakeAiRewriteService(string rewrittenText = "") : IAiRewriteService
    {
        public Task<string> RewriteAsync(string text, RewriteMode mode, CancellationToken cancellationToken = default) =>
            Task.FromResult(string.IsNullOrWhiteSpace(rewrittenText) ? text : rewrittenText);
    }
}
