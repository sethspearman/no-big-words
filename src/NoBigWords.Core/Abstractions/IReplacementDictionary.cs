using NoBigWords.Core.Models;

namespace NoBigWords.Core.Abstractions;

public interface IReplacementDictionary
{
    bool TryGetReplacement(string word, out ReplacementEntry replacement);
}
