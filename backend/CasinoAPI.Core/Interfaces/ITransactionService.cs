using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;

namespace CasinoAPI.Core.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> CreateTransactionAsync(int userId, string transactionType, decimal amount, string? gameType = null, string? description = null);
    Task<PaginatedResult<TransactionDto>> GetUserTransactionsAsync(int userId, int page = 1, int pageSize = 20);
    Task<TransactionDto?> GetTransactionByIdAsync(int transactionId);
}
