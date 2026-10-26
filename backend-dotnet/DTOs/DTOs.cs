namespace UrlShortener.DTOs;

// Auth
public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Name, string Email, string Plan);

// URL
public record CreateShortUrlDto(
    string OriginalUrl,
    string? CustomAlias,
    string? Title,
    string? Description,
    DateTime? ExpiresAt
);

public record UpdateShortUrlDto(
    string? Title,
    string? Description,
    bool? IsActive,
    DateTime? ExpiresAt
);

public record ShortUrlDto(
    int Id,
    string OriginalUrl,
    string ShortCode,
    string ShortUrl,
    string? CustomAlias,
    string? Title,
    string? Description,
    bool IsActive,
    int TotalClicks,
    DateTime? ExpiresAt,
    DateTime CreatedAt
);

// Analytics
public record ClickEventDto(
    int Id,
    string? Country,
    string? Device,
    string? Browser,
    string? Referer,
    DateTime ClickedAt
);

public record UrlAnalyticsDto(
    int TotalClicks,
    int ClicksToday,
    int ClicksThisWeek,
    int ClicksThisMonth,
    List<DailyClickDto> DailyClicks,
    List<DeviceBreakdownDto> DeviceBreakdown,
    List<BrowserBreakdownDto> BrowserBreakdown,
    List<TopRefererDto> TopReferers
);

public record DailyClickDto(string Date, int Clicks);
public record DeviceBreakdownDto(string Device, int Count, double Percentage);
public record BrowserBreakdownDto(string Browser, int Count, double Percentage);
public record TopRefererDto(string Referer, int Count);

// Dashboard
public record DashboardDto(
    int TotalUrls,
    int ActiveUrls,
    int TotalClicks,
    int ClicksToday,
    List<ShortUrlDto> RecentUrls
);

// Pagination
public record PagedResult<T>(List<T> Data, int TotalCount, int Page, int PageSize, int TotalPages);
