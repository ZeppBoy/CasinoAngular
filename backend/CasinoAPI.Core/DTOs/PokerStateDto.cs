namespace CasinoAPI.Core.DTOs;

public class PokerStateDto
{
    public string GameId { get; set; } = string.Empty;
    public List<CardDto> Hand { get; set; } = new();
    public string HandRank { get; set; } = string.Empty;
    public decimal BetAmount { get; set; }
    public decimal WinAmount { get; set; }
    public decimal Payout { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Status { get; set; } = string.Empty; // Playing, Won, Lost
    public bool CanDraw { get; set; }
    public List<int> CardsToHold { get; set; } = new();
}
