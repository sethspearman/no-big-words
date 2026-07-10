namespace NoBigWords.Infrastructure.Options;

public sealed class DataFilesOptions
{
    public required string AllowedWordsPath { get; init; }

    public required string ReplacementsPath { get; init; }
}
