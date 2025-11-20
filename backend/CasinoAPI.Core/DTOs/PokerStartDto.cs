using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class PokerStartDto
{
    [Required]
    [Range(0.50, 1000.00, ErrorMessage = "Bet amount must be between $0.50 and $1,000")]
    public decimal BetAmount { get; set; }
}
