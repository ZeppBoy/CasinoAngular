namespace CasinoAPI.Core.Entities;

public class Transaction
{
    public int TransactionId { get; set; }
    public int UserId { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Deposit, Withdrawal, Bet, Win
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? GameType { get; set; } // SlotMachine, Blackjack, Poker, Roulette
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}
