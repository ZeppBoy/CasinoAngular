using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class RouletteBetDto
{
    [Required(ErrorMessage = "Bet amount is required")]
    [Range(0.50, 1000.00, ErrorMessage = "Bet amount must be between $0.50 and $1,000.00")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Bet type is required")]
    public string BetType { get; set; } = string.Empty; // Number, Red, Black, Even, Odd, High, Low, Dozen, Column

    public int? Number { get; set; } // For straight-up number bets (0-36)
    public string? Range { get; set; } // For dozen (1st, 2nd, 3rd) or column (1st, 2nd, 3rd)
}
