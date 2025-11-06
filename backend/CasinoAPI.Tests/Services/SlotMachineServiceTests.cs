using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;
using CasinoAPI.Core.Services;
using Moq;
using Xunit;

namespace CasinoAPI.Tests.Services;

public class SlotMachineServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly SlotMachineService _slotMachineService;

    public SlotMachineServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTransactionService = new Mock<ITransactionService>();
        _slotMachineService = new SlotMachineService(
            _mockUserRepository.Object,
            _mockTransactionService.Object);
    }

    [Fact]
    public async Task SpinAsync_SufficientBalance_DeductsBetAmount()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act
        var result = await _slotMachineService.SpinAsync(1, 10m);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Reels);
        Assert.Equal(3, result.Reels.Length);
        Assert.All(result.Reels, reel => Assert.Equal(3, reel.Length));
        
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<User>(u => 
            u.Balance <= 1000m)), Times.Once);
        
        _mockTransactionService.Verify(x => x.CreateTransactionAsync(
            1, "Bet", 10m, "SlotMachine", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task SpinAsync_UserNotFound_ThrowsException()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _slotMachineService.SpinAsync(999, 10m));
    }

    [Fact]
    public async Task SpinAsync_InsufficientBalance_ThrowsException()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 5m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _slotMachineService.SpinAsync(1, 10m));
        
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task SpinAsync_ReturnsValidReels()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act
        var result = await _slotMachineService.SpinAsync(1, 10m);

        // Assert
        Assert.Equal(3, result.Reels.Length);
        Assert.All(result.Reels, reel =>
        {
            Assert.Equal(3, reel.Length);
            Assert.All(reel, symbol => Assert.NotEmpty(symbol));
        });
    }

    [Fact]
    public async Task SpinAsync_ReturnsNonNegativeWinAmount()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act
        var result = await _slotMachineService.SpinAsync(1, 10m);

        // Assert
        Assert.True(result.WinAmount >= 0);
    }

    [Fact]
    public async Task SpinAsync_WithWin_CreatesWinTransaction()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act - Spin multiple times to likely get a win
        bool gotWin = false;
        for (int i = 0; i < 100; i++)
        {
            var result = await _slotMachineService.SpinAsync(1, 1m);
            if (result.WinAmount > 0)
            {
                gotWin = true;
                Assert.True(result.WinLines.Count > 0);
                
                // Verify win transaction was created
                _mockTransactionService.Verify(x => x.CreateTransactionAsync(
                    1, "Win", It.IsAny<decimal>(), "SlotMachine", It.IsAny<string>()), Times.AtLeastOnce);
                break;
            }
        }

        // With 100 spins, we should get at least one win statistically
        Assert.True(gotWin, "Expected to get at least one win in 100 spins");
    }

    [Fact]
    public async Task SpinAsync_UpdatesBalanceAfterWin()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act
        var result = await _slotMachineService.SpinAsync(1, 10m);

        // Assert
        Assert.True(result.BalanceAfter >= 0);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task SpinAsync_NoWin_DoesNotCreateWinTransaction()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(),
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act - Spin until we get a no-win result
        for (int i = 0; i < 100; i++)
        {
            // Reset mock
            _mockTransactionService.Invocations.Clear();
            
            var result = await _slotMachineService.SpinAsync(1, 1m);
            if (result.WinAmount == 0)
            {
                // Assert - Should only have bet transaction, no win transaction
                _mockTransactionService.Verify(x => x.CreateTransactionAsync(
                    1, "Bet", 1m, "SlotMachine", It.IsAny<string>()), Times.Once);
                _mockTransactionService.Verify(x => x.CreateTransactionAsync(
                    1, "Win", It.IsAny<decimal>(), "SlotMachine", It.IsAny<string>()), Times.Never);
                return;
            }
        }

        // At least one spin should have resulted in no win
        Assert.True(false, "Expected to get at least one no-win result in 100 spins");
    }
}
