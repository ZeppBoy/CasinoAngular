namespace CasinoAPI.Core.DTOs;

public class TransactionDto
{
    public int TransactionId { get; set; }
    public int UserId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? GameType { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
}
