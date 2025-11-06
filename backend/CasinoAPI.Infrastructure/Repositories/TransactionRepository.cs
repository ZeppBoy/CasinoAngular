using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;
using CasinoAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CasinoAPI.Infrastructure.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly CasinoDbContext _context;

    public TransactionRepository(CasinoDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction?> GetByIdAsync(int transactionId)
    {
        return await _context.Transactions.FindAsync(transactionId);
    }

    public async Task<List<Transaction>> GetByUserIdAsync(int userId, int page, int pageSize)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetCountByUserIdAsync(int userId)
    {
        return await _context.Transactions
            .Where(t => t.UserId == userId)
            .CountAsync();
    }
}
