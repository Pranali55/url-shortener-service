using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.DTOs;
using UrlShortener.Services;

namespace UrlShortener.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UrlsController : ControllerBase
{
    private readonly UrlService _urls;
    public UrlsController(UrlService urls) => _urls = urls;
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
        => Ok(await _urls.GetDashboardAsync(UserId));

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
        => Ok(await _urls.GetAllAsync(UserId, page, pageSize, search));

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var url = await _urls.GetByIdAsync(UserId, id);
        return url == null ? NotFound() : Ok(url);
    }

    [HttpGet("{id}/analytics")]
    public async Task<IActionResult> GetAnalytics(int id)
    {
        var analytics = await _urls.GetAnalyticsAsync(UserId, id);
        return analytics == null ? NotFound() : Ok(analytics);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateShortUrlDto dto)
    {
        try { return Ok(await _urls.CreateAsync(UserId, dto)); }
        catch (Exception ex) { return BadRequest(ex.Message); }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateShortUrlDto dto)
    {
        var result = await _urls.UpdateAsync(UserId, id, dto);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _urls.DeleteAsync(UserId, id);
        return deleted ? NoContent() : NotFound();
    }
}
