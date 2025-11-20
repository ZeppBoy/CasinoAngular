using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class PokerDrawDto
{
    [Required]
    public List<int> CardsToHold { get; set; } = new(); // Indices of cards to keep (0-4)
}
