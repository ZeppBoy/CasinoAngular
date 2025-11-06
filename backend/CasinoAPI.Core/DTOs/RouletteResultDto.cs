namespace CasinoAPI.Core.DTOs;

public class RouletteResultDto
{
    public int WinningNumber { get; set; }
    public string Color { get; set; } = string.Empty; // Red, Black, Green
    public bool IsEven { get; set; }
    public bool IsHigh { get; set; }
    public List<RouletteBetResultDto> BetResults { get; set; } = new();
    public decimal TotalWinAmount { get; set; }
    public decimal BalanceAfter { get; set; }
}

public class RouletteBetResultDto
{
    public string BetType { get; set; } = string.Empty;
    public decimal BetAmount { get; set; }
    public bool IsWin { get; set; }
    public decimal WinAmount { get; set; }
    public decimal Payout { get; set; } // Payout multiplier (e.g., 35:1, 2:1, etc.)
}
