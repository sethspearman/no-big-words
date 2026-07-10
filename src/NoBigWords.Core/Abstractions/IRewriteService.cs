using NoBigWords.Core.Models;

namespace NoBigWords.Core.Abstractions;

public interface IRewriteService
{
    Task<RewriteResponse> RewriteAsync(RewriteRequest request, CancellationToken cancellationToken = default);
}
