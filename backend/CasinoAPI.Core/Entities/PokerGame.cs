namespace CasinoAPI.Core.Entities;

public class PokerGame
{
    public string GameId { get; set; } = Guid.NewGuid().ToString();
    public int UserId { get; set; }
    public decimal BetAmount { get; set; }
    public List<Card> Deck { get; set; } = new();
    public List<Card> Hand { get; set; } = new();
    public string Status { get; set; } = "Playing"; // Playing, Completed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string HandRank { get; set; } = string.Empty;
    public decimal WinAmount { get; set; }
    public bool HasDrawn { get; set; }
}

public class Card
{
    public string Suit { get; set; } = string.Empty; // Hearts, Diamonds, Clubs, Spades
    public string Rank { get; set; } = string.Empty; // 2-10, J, Q, K, A
    public int Value { get; set; } // 2-14 (A=14 for high, 1 for low in straights)

    public override string ToString() => $"{Rank}{GetSuitSymbol()}";

    private string GetSuitSymbol()
    {
        return Suit switch
        {
            "Hearts" => "♥",
            "Diamonds" => "♦",
            "Clubs" => "♣",
            "Spades" => "♠",
            _ => ""
        };
    }
}
