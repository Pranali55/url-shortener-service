using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using UrlShortener.Data;
using UrlShortener.DTOs;
using UrlShortener.Models;

namespace UrlShortener.Services;

public class UrlService
{
    private readonly AppDbContext _db;
    private readonly IConnectionMultiplexer _redis;
    private readonly IConfiguration _config;
    private readonly string _baseUrl;

    public UrlService(AppDbContext db, IConnectionMultiplexer redis, IConfiguration config)
    {
        _db = db;
        _redis = redis;
        _config = config;
        _baseUrl = config["AppSettings__BaseUrl"] ?? "http://localhost:5000";
    }

    public async Task<ShortUrlDto> CreateAsync(int userId, CreateShortUrlDto dto)
    {
        var code = dto.CustomAlias ?? GenerateCode();

        // Check uniqueness
        if (_db.ShortUrls.Any(s => s.ShortCode == code || s.CustomAlias == code))
            throw new Exception("Alias already taken.");

        var shortUrl = new ShortUrl
        {
            UserId = userId,
            OriginalUrl = dto.OriginalUrl,
            ShortCode = code,
            CustomAlias = dto.CustomAlias,
            Title = dto.Title,
            Description = dto.Description,
            ExpiresAt = dto.ExpiresAt
        };

        _db.ShortUrls.Add(shortUrl);
        await _db.SaveChangesAsync();

        // Cache in Redis
        var cache = _redis.GetDatabase();
        await cache.StringSetAsync($"url:{code}", dto.OriginalUrl, TimeSpan.FromHours(24));

        return MapToDto(shortUrl);
    }

    public async Task<string?> ResolveAsync(string code, HttpRequest request)
    {
        // Try Redis cache first
        var cache = _redis.GetDatabase();
        var cached = await cache.StringGetAsync($"url:{code}");
        string? originalUrl = null;

        if (cached.HasValue)
        {
            originalUrl = cached.ToString();
        }
        else
        {
            var shortUrl = await _db.ShortUrls
                .FirstOrDefaultAsync(s => (s.ShortCode == code || s.CustomAlias == code) && s.IsActive);

            if (shortUrl == null) return null;
            if (shortUrl.ExpiresAt.HasValue && shortUrl.ExpiresAt < DateTime.UtcNow) return null;

            originalUrl = shortUrl.OriginalUrl;
            await cache.StringSetAsync($"url:{code}", originalUrl, TimeSpan.FromHours(24));
        }

        // Track click asynchronously
        _ = TrackClickAsync(code, request);

        return originalUrl;
    }

    private async Task TrackClickAsync(string code, HttpRequest request)
    {
        try
        {
            var shortUrl = await _db.ShortUrls.FirstOrDefaultAsync(s => s.ShortCode == code || s.CustomAlias == code);
            if (shortUrl == null) return;

            var ua = request.Headers["User-Agent"].ToString();
            var click = new ClickEvent
            {
                ShortUrlId = shortUrl.Id,
                IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = ua,
                Referer = request.Headers["Referer"].ToString(),
                Device = DetectDevice(ua),
                Browser = DetectBrowser(ua)
            };

            shortUrl.TotalClicks++;
            _db.ClickEvents.Add(click);
            await _db.SaveChangesAsync();
        }
        catch { /* Log in production */ }
    }

    public async Task<PagedResult<ShortUrlDto>> GetAllAsync(int userId, int page, int pageSize, string? search)
    {
        var query = _db.ShortUrls.Where(s => s.UserId == userId);
        if (!string.IsNullOrEmpty(search))
            query = query.Where(s => s.Title!.Contains(search) || s.OriginalUrl.Contains(search) || s.ShortCode.Contains(search));

        var total = await query.CountAsync();
        var items = await query.OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ShortUrlDto>(items.Select(MapToDto).ToList(), total, page, pageSize,
            (int)Math.Ceiling((double)total / pageSize));
    }

    public async Task<ShortUrlDto?> GetByIdAsync(int userId, int id)
    {
        var url = await _db.ShortUrls.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        return url == null ? null : MapToDto(url);
    }

    public async Task<UrlAnalyticsDto?> GetAnalyticsAsync(int userId, int id)
    {
        var url = await _db.ShortUrls.Include(s => s.ClickEvents)
            .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (url == null) return null;

        var clicks = url.ClickEvents.ToList();
        var today = DateTime.UtcNow.Date;

        var daily = Enumerable.Range(0, 30)
            .Select(i => today.AddDays(-29 + i))
            .Select(d => new DailyClickDto(d.ToString("MMM dd"), clicks.Count(c => c.ClickedAt.Date == d)))
            .ToList();

        var devices = clicks.GroupBy(c => c.Device ?? "Unknown")
            .Select(g => new DeviceBreakdownDto(g.Key, g.Count(), Math.Round((double)g.Count() / clicks.Count * 100, 1)))
            .ToList();

        var browsers = clicks.GroupBy(c => c.Browser ?? "Unknown")
            .Select(g => new BrowserBreakdownDto(g.Key, g.Count(), Math.Round((double)g.Count() / clicks.Count * 100, 1)))
            .ToList();

        var referers = clicks.Where(c => !string.IsNullOrEmpty(c.Referer))
            .GroupBy(c => c.Referer!)
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => new TopRefererDto(g.Key, g.Count()))
            .ToList();

        return new UrlAnalyticsDto(
            clicks.Count,
            clicks.Count(c => c.ClickedAt.Date == today),
            clicks.Count(c => c.ClickedAt >= today.AddDays(-7)),
            clicks.Count(c => c.ClickedAt >= today.AddDays(-30)),
            daily, devices, browsers, referers
        );
    }

    public async Task<DashboardDto> GetDashboardAsync(int userId)
    {
        var urls = await _db.ShortUrls.Where(s => s.UserId == userId).ToListAsync();
        var today = DateTime.UtcNow.Date;
        var clicksToday = await _db.ClickEvents
            .CountAsync(c => c.ShortUrl.UserId == userId && c.ClickedAt.Date == today);

        var recent = urls.OrderByDescending(u => u.CreatedAt).Take(5).Select(MapToDto).ToList();

        return new DashboardDto(
            urls.Count,
            urls.Count(u => u.IsActive),
            urls.Sum(u => u.TotalClicks),
            clicksToday,
            recent
        );
    }

    public async Task<ShortUrlDto?> UpdateAsync(int userId, int id, UpdateShortUrlDto dto)
    {
        var url = await _db.ShortUrls.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (url == null) return null;
        if (dto.Title != null) url.Title = dto.Title;
        if (dto.Description != null) url.Description = dto.Description;
        if (dto.IsActive.HasValue) url.IsActive = dto.IsActive.Value;
        if (dto.ExpiresAt.HasValue) url.ExpiresAt = dto.ExpiresAt.Value;
        await _db.SaveChangesAsync();

        // Invalidate cache
        var cache = _redis.GetDatabase();
        await cache.KeyDeleteAsync($"url:{url.ShortCode}");

        return MapToDto(url);
    }

    public async Task<bool> DeleteAsync(int userId, int id)
    {
        var url = await _db.ShortUrls.FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);
        if (url == null) return false;

        var cache = _redis.GetDatabase();
        await cache.KeyDeleteAsync($"url:{url.ShortCode}");

        _db.ShortUrls.Remove(url);
        await _db.SaveChangesAsync();
        return true;
    }

    private ShortUrlDto MapToDto(ShortUrl s) => new(
        s.Id, s.OriginalUrl, s.ShortCode,
        $"{_baseUrl}/r/{s.ShortCode}",
        s.CustomAlias, s.Title, s.Description,
        s.IsActive, s.TotalClicks, s.ExpiresAt, s.CreatedAt
    );

    private static string GenerateCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    private static string DetectDevice(string ua)
    {
        if (ua.Contains("Mobile") || ua.Contains("Android")) return "Mobile";
        if (ua.Contains("Tablet") || ua.Contains("iPad")) return "Tablet";
        return "Desktop";
    }

    private static string DetectBrowser(string ua)
    {
        if (ua.Contains("Chrome")) return "Chrome";
        if (ua.Contains("Firefox")) return "Firefox";
        if (ua.Contains("Safari")) return "Safari";
        if (ua.Contains("Edge")) return "Edge";
        return "Other";
    }
}
