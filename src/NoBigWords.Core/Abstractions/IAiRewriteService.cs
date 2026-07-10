using NoBigWords.Core.Models;

namespace NoBigWords.Core.Abstractions;

public interface IAiRewriteService
{
    Task<string> RewriteAsync(string text, RewriteMode mode, CancellationToken cancellationToken = default);
}
