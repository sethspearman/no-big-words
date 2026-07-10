using System.Text.Json;
using NoBigWords.Infrastructure.Providers;

namespace NoBigWords.Tests;

public sealed class GeneratedReplacementDictionaryTests
{
    private static readonly string GeneratedDictionaryPath =
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "replacements.generated.json"));

    [Theory]
    [InlineData("government", "group in power")]
    [InlineData("research", "study")]
    [InlineData("technology", "machine way")]
    [InlineData("website", "computer place")]
    [InlineData("difficult", "hard")]
    public void GeneratedDictionary_ContainsExpectedHighValueReplacements(string word, string expectedReplacement)
    {
        var dictionary = new JsonReplacementDictionary(GeneratedDictionaryPath);

        var found = dictionary.TryGetReplacement(word, out var entry);

        Assert.True(found);
        Assert.Equal(expectedReplacement, entry.Replacement);
        Assert.InRange(entry.Confidence, 0.6, 1.0);
    }

    [Fact]
    public void GeneratedDictionary_HasEntriesForEveryCandidateWord()
    {
        var generatedJson = JsonDocument.Parse(File.ReadAllText(GeneratedDictionaryPath));
        var candidateWords = File.ReadAllLines(
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "data", "top-15000-common-words-excluding-allowed.txt")))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        Assert.Equal(candidateWords.Count, generatedJson.RootElement.EnumerateObject().Count());

        foreach (var word in candidateWords.Take(250))
        {
            Assert.True(generatedJson.RootElement.TryGetProperty(word, out _), $"Missing generated replacement for '{word}'.");
        }
    }
}
