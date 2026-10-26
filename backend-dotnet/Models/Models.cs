namespace UrlShortener.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Plan { get; set; } = "Free"; // Free, Pro
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<ShortUrl> ShortUrls { get; set; } = new List<ShortUrl>();
}

public class ShortUrl
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public string? CustomAlias { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public int TotalClicks { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public User User { get; set; } = null!;
    public ICollection<ClickEvent> ClickEvents { get; set; } = new List<ClickEvent>();
}

public class ClickEvent
{
    public int Id { get; set; }
    public int ShortUrlId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
    public string? Country { get; set; }
    public string? Device { get; set; } // Desktop, Mobile, Tablet
    public string? Browser { get; set; }
    public DateTime ClickedAt { get; set; } = DateTime.UtcNow;
    public ShortUrl ShortUrl { get; set; } = null!;
}
