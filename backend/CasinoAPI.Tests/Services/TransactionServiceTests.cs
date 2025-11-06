using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;
using CasinoAPI.Core.Services;
using Moq;
using Xunit;

namespace CasinoAPI.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<ITransactionRepository> _mockTransactionRepository;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        _mockTransactionRepository = new Mock<ITransactionRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _transactionService = new TransactionService(
            _mockTransactionRepository.Object, 
            _mockUserRepository.Object);
    }

    [Fact]
    public async Task CreateTransactionAsync_ValidTransaction_CreatesAndReturnsDto()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Balance = 1500m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        var transaction = new Transaction
        {
            TransactionId = 1,
            UserId = 1,
            TransactionType = "Deposit",
            Amount = 500m,
            BalanceBefore = 1000m,
            BalanceAfter = 1500m,
            CreatedDate = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockTransactionRepository.Setup(x => x.CreateAsync(It.IsAny<Transaction>()))
            .ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.CreateTransactionAsync(
            1, "Deposit", 500m, null, "Test deposit");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TransactionId);
        Assert.Equal("Deposit", result.TransactionType);
        Assert.Equal(500m, result.Amount);
        _mockTransactionRepository.Verify(x => x.CreateAsync(It.IsAny<Transaction>()), Times.Once);
    }

    [Fact]
    public async Task CreateTransactionAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _transactionService.CreateTransactionAsync(999, "Deposit", 100m));
    }

    [Fact]
    public async Task GetUserTransactionsAsync_ReturnsPagedResults()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new Transaction
            {
                TransactionId = 1,
                UserId = 1,
                TransactionType = "Deposit",
                Amount = 500m,
                BalanceBefore = 1000m,
                BalanceAfter = 1500m,
                CreatedDate = DateTime.UtcNow
            },
            new Transaction
            {
                TransactionId = 2,
                UserId = 1,
                TransactionType = "Withdrawal",
                Amount = 200m,
                BalanceBefore = 1500m,
                BalanceAfter = 1300m,
                CreatedDate = DateTime.UtcNow
            }
        };

        _mockTransactionRepository.Setup(x => x.GetByUserIdAsync(1, 1, 20))
            .ReturnsAsync(transactions);
        _mockTransactionRepository.Setup(x => x.GetCountByUserIdAsync(1))
            .ReturnsAsync(2);

        // Act
        var result = await _transactionService.GetUserTransactionsAsync(1, 1, 20);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.False(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    [Fact]
    public async Task GetUserTransactionsAsync_WithPagination_CalculatesCorrectly()
    {
        // Arrange
        var transactions = new List<Transaction>();
        _mockTransactionRepository.Setup(x => x.GetByUserIdAsync(1, 2, 10))
            .ReturnsAsync(transactions);
        _mockTransactionRepository.Setup(x => x.GetCountByUserIdAsync(1))
            .ReturnsAsync(25);

        // Act
        var result = await _transactionService.GetUserTransactionsAsync(1, 2, 10);

        // Assert
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_ExistingTransaction_ReturnsDto()
    {
        // Arrange
        var transaction = new Transaction
        {
            TransactionId = 1,
            UserId = 1,
            TransactionType = "Deposit",
            Amount = 500m,
            BalanceBefore = 1000m,
            BalanceAfter = 1500m,
            Description = "Test deposit",
            CreatedDate = DateTime.UtcNow
        };

        _mockTransactionRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(transaction);

        // Act
        var result = await _transactionService.GetTransactionByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TransactionId);
        Assert.Equal("Deposit", result.TransactionType);
        Assert.Equal(500m, result.Amount);
        Assert.Equal("Test deposit", result.Description);
    }

    [Fact]
    public async Task GetTransactionByIdAsync_NonExistingTransaction_ReturnsNull()
    {
        // Arrange
        _mockTransactionRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Transaction?)null);

        // Act
        var result = await _transactionService.GetTransactionByIdAsync(999);

        // Assert
        Assert.Null(result);
    }
}
