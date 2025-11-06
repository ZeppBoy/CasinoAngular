using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class SlotSpinDto
{
    [Required(ErrorMessage = "Bet amount is required")]
    [Range(0.10, 1000.00, ErrorMessage = "Bet amount must be between $0.10 and $1,000.00")]
    public decimal BetAmount { get; set; }
}
