namespace NoBigWords.Core.Abstractions;

public interface IAllowedWordsProvider
{
    bool IsAllowed(string word);
}
