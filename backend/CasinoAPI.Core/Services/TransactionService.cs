using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;

namespace CasinoAPI.Core.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserRepository _userRepository;

    public TransactionService(ITransactionRepository transactionRepository, IUserRepository userRepository)
    {
        _transactionRepository = transactionRepository;
        _userRepository = userRepository;
    }

    public async Task<TransactionDto> CreateTransactionAsync(
        int userId, 
        string transactionType, 
        decimal amount, 
        string? gameType = null, 
        string? description = null)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        var balanceBefore = transactionType == "Deposit" 
            ? user.Balance - amount 
            : transactionType == "Withdrawal" 
                ? user.Balance + amount 
                : user.Balance;

        var transaction = new Transaction
        {
            UserId = userId,
            TransactionType = transactionType,
            Amount = amount,
            BalanceBefore = balanceBefore,
            BalanceAfter = user.Balance,
            GameType = gameType,
            Description = description,
            CreatedDate = DateTime.UtcNow
        };

        var createdTransaction = await _transactionRepository.CreateAsync(transaction);

        return new TransactionDto
        {
            TransactionId = createdTransaction.TransactionId,
            UserId = createdTransaction.UserId,
            TransactionType = createdTransaction.TransactionType,
            Amount = createdTransaction.Amount,
            BalanceBefore = createdTransaction.BalanceBefore,
            BalanceAfter = createdTransaction.BalanceAfter,
            GameType = createdTransaction.GameType,
            Description = createdTransaction.Description,
            CreatedDate = createdTransaction.CreatedDate
        };
    }

    public async Task<PaginatedResult<TransactionDto>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100;

        var totalCount = await _transactionRepository.GetCountByUserIdAsync(userId);
        var transactions = await _transactionRepository.GetByUserIdAsync(userId, page, pageSize);

        var items = transactions.Select(t => new TransactionDto
        {
            TransactionId = t.TransactionId,
            UserId = t.UserId,
            TransactionType = t.TransactionType,
            Amount = t.Amount,
            BalanceBefore = t.BalanceBefore,
            BalanceAfter = t.BalanceAfter,
            GameType = t.GameType,
            Description = t.Description,
            CreatedDate = t.CreatedDate
        }).ToList();

        return new PaginatedResult<TransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(int transactionId)
    {
        var transaction = await _transactionRepository.GetByIdAsync(transactionId);
        
        if (transaction == null)
        {
            return null;
        }

        return new TransactionDto
        {
            TransactionId = transaction.TransactionId,
            UserId = transaction.UserId,
            TransactionType = transaction.TransactionType,
            Amount = transaction.Amount,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            GameType = transaction.GameType,
            Description = transaction.Description,
            CreatedDate = transaction.CreatedDate
        };
    }
}
