using Microsoft.AspNetCore.Mvc;
using NoBigWords.Core.Abstractions;
using NoBigWords.Core.Models;

namespace NoBigWords.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class RewriteController(IRewriteService rewriteService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<RewriteResponse>> RewriteAsync(
        [FromBody] RewriteRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required.");
        }

        var response = await rewriteService.RewriteAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("/api/validate")]
    public async Task<ActionResult<ValidationSummary>> ValidateAsync(
        [FromBody] RewriteRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest("Text is required.");
        }

        var response = await rewriteService.RewriteAsync(request with { Mode = RewriteMode.NoBigWords }, cancellationToken);
        return Ok(response.Validation);
    }
}
