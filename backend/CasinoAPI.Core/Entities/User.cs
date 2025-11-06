namespace CasinoAPI.Core.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public decimal Balance { get; set; } = 1000.00m;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public ICollection<GameSession> GameSessions { get; set; } = new List<GameSession>();
    public ICollection<GameHistory> GameHistories { get; set; } = new List<GameHistory>();
}
