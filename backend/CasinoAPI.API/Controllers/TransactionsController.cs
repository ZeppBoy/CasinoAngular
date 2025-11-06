using System.Security.Claims;
using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasinoAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(ITransactionService transactionService, ILogger<TransactionsController> logger)
    {
        _transactionService = transactionService;
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

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResult<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResult<TransactionDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            var transactions = await _transactionService.GetUserTransactionsAsync(userId, page, pageSize);
            return Ok(transactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            return StatusCode(500, new { message = "An error occurred while retrieving transactions" });
        }
    }

    [HttpGet("{transactionId}")]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionDto>> GetTransaction(int transactionId)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(transactionId);
            
            if (transaction == null)
            {
                return NotFound(new { message = "Transaction not found" });
            }

            var userId = GetCurrentUserId();
            if (transaction.UserId != userId)
            {
                return Forbid();
            }

            return Ok(transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction");
            return StatusCode(500, new { message = "An error occurred while retrieving transaction" });
        }
    }
}
