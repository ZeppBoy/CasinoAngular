namespace CasinoAPI.Core.Entities;

public class GameHistory
{
    public int GameHistoryId { get; set; }
    public int SessionId { get; set; }
    public int UserId { get; set; }
    public string GameType { get; set; } = string.Empty;
    public decimal BetAmount { get; set; }
    public decimal WinAmount { get; set; } = 0;
    public string? GameData { get; set; } // JSON data for game-specific details
    public DateTime PlayedDate { get; set; } = DateTime.UtcNow;

    public GameSession Session { get; set; } = null!;
    public User User { get; set; } = null!;
}
