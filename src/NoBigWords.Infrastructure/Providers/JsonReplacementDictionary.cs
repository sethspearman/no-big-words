using System.Text.Json;
using NoBigWords.Core.Abstractions;
using NoBigWords.Core.Models;

namespace NoBigWords.Infrastructure.Providers;

public sealed class JsonReplacementDictionary : IReplacementDictionary
{
    private readonly IReadOnlyDictionary<string, ReplacementEntry> _entries;

    public JsonReplacementDictionary(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var entries = JsonSerializer.Deserialize<Dictionary<string, ReplacementEntry>>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        }) ?? [];

        _entries = entries.ToDictionary(
            pair => pair.Key.ToLowerInvariant(),
            pair => pair.Value,
            StringComparer.Ordinal);
    }

    public bool TryGetReplacement(string word, out ReplacementEntry replacement) =>
        _entries.TryGetValue(word, out replacement!);
}
