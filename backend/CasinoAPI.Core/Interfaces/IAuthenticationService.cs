using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;

namespace CasinoAPI.Core.Interfaces;

public interface IAuthenticationService
{
    Task<TokenDto> RegisterAsync(RegisterDto registerDto);
    Task<TokenDto> LoginAsync(LoginDto loginDto);
    Task<TokenDto> RefreshTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateJwtToken(User user);
    string GenerateRefreshToken();
}
