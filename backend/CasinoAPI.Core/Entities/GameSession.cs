namespace CasinoAPI.Core.Entities;

public class GameSession
{
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public string GameType { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public int TotalBets { get; set; } = 0;
    public decimal TotalWinnings { get; set; } = 0;
    public decimal TotalLosses { get; set; } = 0;

    public User User { get; set; } = null!;
    public ICollection<GameHistory> GameHistories { get; set; } = new List<GameHistory>();
}
