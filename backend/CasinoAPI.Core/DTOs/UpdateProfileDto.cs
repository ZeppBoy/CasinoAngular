using System.ComponentModel.DataAnnotations;

namespace CasinoAPI.Core.DTOs;

public class UpdateProfileDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
    public string? Username { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [StringLength(100, ErrorMessage = "Email must not exceed 100 characters")]
    public string? Email { get; set; }
}
