namespace CasinoAPI.Core.DTOs;

public class SlotResultDto
{
    public string[][] Reels { get; set; } = Array.Empty<string[]>();
    public decimal WinAmount { get; set; }
    public List<WinLineDto> WinLines { get; set; } = new();
    public decimal BalanceAfter { get; set; }
    public bool IsJackpot { get; set; }
}

public class WinLineDto
{
    public int LineNumber { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Payout { get; set; }
}
