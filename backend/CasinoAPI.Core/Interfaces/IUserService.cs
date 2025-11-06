using CasinoAPI.Core.DTOs;

namespace CasinoAPI.Core.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto updateDto);
    Task<BalanceDto> GetBalanceAsync(int userId);
    Task<BalanceDto> DepositAsync(int userId, DepositDto depositDto);
    Task<BalanceDto> WithdrawAsync(int userId, WithdrawDto withdrawDto);
}
