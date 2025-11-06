namespace CasinoAPI.Core.DTOs;

public class BlackjackStateDto
{
    public string GameId { get; set; } = string.Empty;
    public List<CardDto> PlayerHand { get; set; } = new();
    public List<CardDto> DealerHand { get; set; } = new();
    public int PlayerHandValue { get; set; }
    public int DealerHandValue { get; set; }
    public bool DealerShowingHoleCard { get; set; }
    public string Status { get; set; } = string.Empty; // Playing, PlayerBlackjack, DealerBlackjack, PlayerBust, DealerBust, PlayerWin, DealerWin, Push
    public decimal? WinAmount { get; set; }
    public decimal BalanceAfter { get; set; }
    public bool CanHit { get; set; }
    public bool CanStand { get; set; }
    public bool CanDouble { get; set; }
}
