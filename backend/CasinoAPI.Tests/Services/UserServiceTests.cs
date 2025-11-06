using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;
using CasinoAPI.Core.Services;
using Moq;
using Xunit;

namespace CasinoAPI.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITransactionService> _mockTransactionService;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTransactionService = new Mock<ITransactionService>();
        _userService = new UserService(_mockUserRepository.Object, _mockTransactionService.Object);
    }

    [Fact]
    public async Task GetProfileAsync_ExistingUser_ReturnsProfile()
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

        // Act
        var result = await _userService.GetProfileAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserId, result.UserId);
        Assert.Equal(user.Username, result.Username);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Balance, result.Balance);
    }

    [Fact]
    public async Task GetProfileAsync_NonExistingUser_ThrowsException()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            _userService.GetProfileAsync(999));
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidUpdate_UpdatesAndReturnsProfile()
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

        var updateDto = new UpdateProfileDto
        {
            Email = "newemail@example.com"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.EmailExistsAsync(updateDto.Email!)).ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

        // Act
        var result = await _userService.UpdateProfileAsync(1, updateDto);

        // Assert
        Assert.Equal(updateDto.Email, result.Email);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileAsync_DuplicateEmail_ThrowsException()
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

        var updateDto = new UpdateProfileDto
        {
            Email = "existing@example.com"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.EmailExistsAsync(updateDto.Email!)).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _userService.UpdateProfileAsync(1, updateDto));
        
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetBalanceAsync_ExistingUser_ReturnsBalance()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 1500.50m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _userService.GetBalanceAsync(1);

        // Assert
        Assert.Equal(1500.50m, result.Balance);
    }

    [Fact]
    public async Task DepositAsync_ValidAmount_UpdatesBalanceAndCreatesTransaction()
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

        var depositDto = new DepositDto
        {
            Amount = 500m,
            Description = "Test deposit"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(), 
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act
        var result = await _userService.DepositAsync(1, depositDto);

        // Assert
        Assert.Equal(1500m, result.Balance);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockTransactionService.Verify(x => x.CreateTransactionAsync(
            1, "Deposit", 500m, null, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_SufficientBalance_UpdatesBalanceAndCreatesTransaction()
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

        var withdrawDto = new WithdrawDto
        {
            Amount = 300m,
            Description = "Test withdrawal"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);
        _mockTransactionService.Setup(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(), 
            It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new TransactionDto());

        // Act
        var result = await _userService.WithdrawAsync(1, withdrawDto);

        // Assert
        Assert.Equal(700m, result.Balance);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        _mockTransactionService.Verify(x => x.CreateTransactionAsync(
            1, "Withdrawal", 300m, null, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_InsufficientBalance_ThrowsException()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            Balance = 100m,
            CreatedDate = DateTime.UtcNow,
            IsActive = true
        };

        var withdrawDto = new WithdrawDto
        {
            Amount = 500m,
            Description = "Test withdrawal"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _userService.WithdrawAsync(1, withdrawDto));
        
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Never);
        _mockTransactionService.Verify(x => x.CreateTransactionAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<decimal>(), 
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
