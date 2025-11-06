namespace CasinoAPI.Core.Entities;

public class BlackjackGame
{
    public string GameId { get; set; } = Guid.NewGuid().ToString();
    public int UserId { get; set; }
    public decimal BetAmount { get; set; }
    public List<string> PlayerCards { get; set; } = new();
    public List<string> DealerCards { get; set; } = new();
    public string Status { get; set; } = "Playing";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
