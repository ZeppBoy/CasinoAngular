using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class BlackjackStartDto
{
    [Required(ErrorMessage = "Bet amount is required")]
    [Range(0.50, 1000.00, ErrorMessage = "Bet amount must be between $0.50 and $1,000.00")]
    public decimal BetAmount { get; set; }
}
