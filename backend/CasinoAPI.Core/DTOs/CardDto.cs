namespace CasinoAPI.Core.DTOs;

public class CardDto
{
    public string Suit { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
    public int Value { get; set; }
}
