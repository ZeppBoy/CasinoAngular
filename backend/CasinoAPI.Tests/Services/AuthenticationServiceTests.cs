using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;
using CasinoAPI.Core.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CasinoAPI.Tests.Services;

public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly AuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockConfiguration = new Mock<IConfiguration>();

        // Setup mock configuration
        var mockJwtSection = new Mock<IConfigurationSection>();
        mockJwtSection.Setup(x => x["SecretKey"]).Returns("YourSuperSecretKeyForJWTTokenGeneration123!");
        mockJwtSection.Setup(x => x["Issuer"]).Returns("CasinoAPI");
        mockJwtSection.Setup(x => x["Audience"]).Returns("CasinoClient");
        mockJwtSection.Setup(x => x["ExpirationMinutes"]).Returns("60");

        _mockConfiguration.Setup(x => x.GetSection("JwtSettings")).Returns(mockJwtSection.Object);
        _mockConfiguration.Setup(x => x["JwtSettings:SecretKey"]).Returns("YourSuperSecretKeyForJWTTokenGeneration123!");
        _mockConfiguration.Setup(x => x["JwtSettings:Issuer"]).Returns("CasinoAPI");
        _mockConfiguration.Setup(x => x["JwtSettings:Audience"]).Returns("CasinoClient");
        _mockConfiguration.Setup(x => x["JwtSettings:ExpirationMinutes"]).Returns("60");

        _authService = new AuthenticationService(_mockUserRepository.Object, _mockConfiguration.Object);
    }

    [Fact]
    public async Task RegisterAsync_ValidUser_ReturnsTokenDto()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        _mockUserRepository.Setup(x => x.UsernameExistsAsync(registerDto.Username))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.EmailExistsAsync(registerDto.Email))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync((User user) =>
            {
                user.UserId = 1;
                return user;
            });

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);
        Assert.True(result.ExpiresIn > 0);
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateUsername_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        _mockUserRepository.Setup(x => x.UsernameExistsAsync(registerDto.Username))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.RegisterAsync(registerDto));
        
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsInvalidOperationException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            Username = "newuser",
            Email = "existing@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        _mockUserRepository.Setup(x => x.UsernameExistsAsync(registerDto.Username))
            .ReturnsAsync(false);
        _mockUserRepository.Setup(x => x.EmailExistsAsync(registerDto.Email))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _authService.RegisterAsync(registerDto));
        
        _mockUserRepository.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenDto()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = "Test123!"
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = passwordHash,
            PasswordSalt = "salt",
            IsActive = true,
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Token);
        Assert.NotEmpty(result.RefreshToken);
        _mockUserRepository.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidUsername_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UsernameOrEmail = "nonexistent",
            Password = "Test123!"
        };

        _mockUserRepository.Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = "WrongPassword!"
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword", BCrypt.Net.BCrypt.GenerateSalt(12));
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = passwordHash,
            PasswordSalt = "salt",
            IsActive = true,
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = "Test123!"
        };

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password, BCrypt.Net.BCrypt.GenerateSalt(12));
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = passwordHash,
            PasswordSalt = "salt",
            IsActive = false,
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow
        };

        _mockUserRepository.Setup(x => x.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _authService.LoginAsync(loginDto));
    }

    [Fact]
    public void GenerateJwtToken_ValidUser_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        var token = _authService.GenerateJwtToken(user);

        // Assert
        Assert.NotEmpty(token);
        Assert.Contains(".", token);
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsNonEmptyString()
    {
        // Act
        var refreshToken = _authService.GenerateRefreshToken();

        // Assert
        Assert.NotEmpty(refreshToken);
    }

    [Fact]
    public async Task ValidateTokenAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            Balance = 1000m,
            CreatedDate = DateTime.UtcNow
        };

        var token = _authService.GenerateJwtToken(user);

        // Act
        var isValid = await _authService.ValidateTokenAsync(token);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public async Task ValidateTokenAsync_InvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.string";

        // Act
        var isValid = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        Assert.False(isValid);
    }
}
