using Microsoft.AspNetCore.Mvc;
using UrlShortener.Services;

namespace UrlShortener.Controllers;

[ApiController]
public class RedirectController : ControllerBase
{
    private readonly UrlService _urls;
    public RedirectController(UrlService urls) => _urls = urls;

    [HttpGet("/r/{code}")]
    public async Task<IActionResult> Redirect(string code)
    {
        var originalUrl = await _urls.ResolveAsync(code, Request);
        if (originalUrl == null) return NotFound("Short URL not found or expired.");
        return Redirect(originalUrl);
    }
}
