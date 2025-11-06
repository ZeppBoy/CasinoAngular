using CasinoAPI.Core.Entities;

namespace CasinoAPI.Core.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction> CreateAsync(Transaction transaction);
    Task<Transaction?> GetByIdAsync(int transactionId);
    Task<List<Transaction>> GetByUserIdAsync(int userId, int page, int pageSize);
    Task<int> GetCountByUserIdAsync(int userId);
}
