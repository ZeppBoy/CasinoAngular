using System.Security.Claims;
using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasinoAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user token");
        }
        return userId;
    }

    [HttpGet("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        try
        {
            var userId = GetCurrentUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Profile not found: {Message}", ex.Message);
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving profile");
            return StatusCode(500, new { message = "An error occurred while retrieving profile" });
        }
    }

    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var profile = await _userService.UpdateProfileAsync(userId, updateDto);
            _logger.LogInformation("User {UserId} updated profile", userId);
            
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Profile update failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile");
            return StatusCode(500, new { message = "An error occurred while updating profile" });
        }
    }

    [HttpGet("balance")]
    [ProducesResponseType(typeof(BalanceDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BalanceDto>> GetBalance()
    {
        try
        {
            var userId = GetCurrentUserId();
            var balance = await _userService.GetBalanceAsync(userId);
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving balance");
            return StatusCode(500, new { message = "An error occurred while retrieving balance" });
        }
    }

    [HttpPost("deposit")]
    [ProducesResponseType(typeof(BalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BalanceDto>> Deposit([FromBody] DepositDto depositDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var balance = await _userService.DepositAsync(userId, depositDto);
            _logger.LogInformation("User {UserId} deposited {Amount}", userId, depositDto.Amount);
            
            return Ok(balance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing deposit");
            return StatusCode(500, new { message = "An error occurred while processing deposit" });
        }
    }

    [HttpPost("withdraw")]
    [ProducesResponseType(typeof(BalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BalanceDto>> Withdraw([FromBody] WithdrawDto withdrawDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var balance = await _userService.WithdrawAsync(userId, withdrawDto);
            _logger.LogInformation("User {UserId} withdrew {Amount}", userId, withdrawDto.Amount);
            
            return Ok(balance);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Withdrawal failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing withdrawal");
            return StatusCode(500, new { message = "An error occurred while processing withdrawal" });
        }
    }
}
