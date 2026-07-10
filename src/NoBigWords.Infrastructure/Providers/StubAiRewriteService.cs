using NoBigWords.Core.Abstractions;
using NoBigWords.Core.Models;

namespace NoBigWords.Infrastructure.Providers;

public sealed class StubAiRewriteService : IAiRewriteService
{
    public Task<string> RewriteAsync(string text, RewriteMode mode, CancellationToken cancellationToken = default)
    {
        var rewritten = mode == RewriteMode.ExplainLikeImTen
            ? $"AI mode placeholder: {text}"
            : text;

        return Task.FromResult(rewritten);
    }
}
