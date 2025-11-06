namespace CasinoAPI.Core.DTOs;

public class TokenDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int ExpiresIn { get; set; }
    public UserProfileDto User { get; set; } = null!;
}
