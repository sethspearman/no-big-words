using NoBigWords.Core.Abstractions;

namespace NoBigWords.Infrastructure.Providers;

public sealed class FileAllowedWordsProvider : IAllowedWordsProvider
{
    private readonly HashSet<string> _allowedWords;

    public FileAllowedWordsProvider(string filePath)
    {
        _allowedWords = File.ReadAllLines(filePath)
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.ToLowerInvariant())
            .ToHashSet(StringComparer.Ordinal);
    }

    public bool IsAllowed(string word) => _allowedWords.Contains(word);
}
