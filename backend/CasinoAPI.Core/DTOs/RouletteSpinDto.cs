using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class RouletteSpinDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one bet is required")]
    public List<RouletteBetDto> Bets { get; set; } = new();
}
