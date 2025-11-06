using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class WithdrawDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 10000.00, ErrorMessage = "Withdrawal amount must be between $0.01 and $10,000.00")]
    public decimal Amount { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }
}
