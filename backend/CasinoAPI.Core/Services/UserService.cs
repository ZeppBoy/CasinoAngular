using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Interfaces;

namespace CasinoAPI.Core.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionService _transactionService;

    public UserService(IUserRepository userRepository, ITransactionService transactionService)
    {
        _userRepository = userRepository;
        _transactionService = transactionService;
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        return new UserProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Balance = user.Balance,
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto updateDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Update username if provided and different
        if (!string.IsNullOrWhiteSpace(updateDto.Username) && updateDto.Username != user.Username)
        {
            if (await _userRepository.UsernameExistsAsync(updateDto.Username))
            {
                throw new InvalidOperationException("Username already exists");
            }
            user.Username = updateDto.Username;
        }

        // Update email if provided and different
        if (!string.IsNullOrWhiteSpace(updateDto.Email) && updateDto.Email != user.Email)
        {
            if (await _userRepository.EmailExistsAsync(updateDto.Email))
            {
                throw new InvalidOperationException("Email already exists");
            }
            user.Email = updateDto.Email;
        }

        await _userRepository.UpdateAsync(user);

        return new UserProfileDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Balance = user.Balance,
            CreatedDate = user.CreatedDate,
            LastLoginDate = user.LastLoginDate
        };
    }

    public async Task<BalanceDto> GetBalanceAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        return new BalanceDto
        {
            Balance = user.Balance
        };
    }

    public async Task<BalanceDto> DepositAsync(int userId, DepositDto depositDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var balanceBefore = user.Balance;
        user.Balance += depositDto.Amount;

        await _userRepository.UpdateAsync(user);

        // Create transaction record
        await _transactionService.CreateTransactionAsync(
            userId,
            "Deposit",
            depositDto.Amount,
            null,
            depositDto.Description ?? "Deposit to account"
        );

        return new BalanceDto
        {
            Balance = user.Balance
        };
    }

    public async Task<BalanceDto> WithdrawAsync(int userId, WithdrawDto withdrawDto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Balance < withdrawDto.Amount)
        {
            throw new InvalidOperationException("Insufficient balance");
        }

        var balanceBefore = user.Balance;
        user.Balance -= withdrawDto.Amount;

        await _userRepository.UpdateAsync(user);

        // Create transaction record
        await _transactionService.CreateTransactionAsync(
            userId,
            "Withdrawal",
            withdrawDto.Amount,
            null,
            withdrawDto.Description ?? "Withdrawal from account"
        );

        return new BalanceDto
        {
            Balance = user.Balance
        };
    }
}
