using System.Security.Claims;
using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CasinoAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamesController : ControllerBase
{
    private readonly ISlotMachineService _slotMachineService;
    private readonly IBlackjackService _blackjackService;
    private readonly IRouletteService _rouletteService;
    private readonly IPokerService _pokerService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        ISlotMachineService slotMachineService, 
        IBlackjackService blackjackService,
        IRouletteService rouletteService,
        IPokerService pokerService,
        ILogger<GamesController> logger)
    {
        _slotMachineService = slotMachineService;
        _blackjackService = blackjackService;
        _rouletteService = rouletteService;
        _pokerService = pokerService;
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

    [HttpPost("slot/spin")]
    [ProducesResponseType(typeof(SlotResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SlotResultDto>> SpinSlotMachine([FromBody] SlotSpinDto spinDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _slotMachineService.SpinAsync(userId, spinDto.BetAmount);
            
            _logger.LogInformation(
                "User {UserId} played slot machine - Bet: {BetAmount}, Win: {WinAmount}, Jackpot: {IsJackpot}", 
                userId, spinDto.BetAmount, result.WinAmount, result.IsJackpot);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Slot machine spin failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during slot machine spin");
            return StatusCode(500, new { message = "An error occurred during slot machine spin" });
        }
    }

    [HttpPost("blackjack/start")]
    [ProducesResponseType(typeof(BlackjackStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BlackjackStateDto>> StartBlackjack([FromBody] BlackjackStartDto startDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _blackjackService.StartGameAsync(userId, startDto.BetAmount);
            
            _logger.LogInformation(
                "User {UserId} started blackjack - Bet: {BetAmount}, GameId: {GameId}", 
                userId, startDto.BetAmount, result.GameId);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Blackjack start failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting blackjack");
            return StatusCode(500, new { message = "An error occurred starting blackjack" });
        }
    }

    [HttpPost("blackjack/{gameId}/hit")]
    [ProducesResponseType(typeof(BlackjackStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BlackjackStateDto>> HitBlackjack(string gameId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _blackjackService.HitAsync(userId, gameId);
            
            _logger.LogInformation(
                "User {UserId} hit in blackjack game {GameId}", 
                userId, gameId);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Blackjack hit failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized blackjack hit: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during blackjack hit");
            return StatusCode(500, new { message = "An error occurred during hit" });
        }
    }

    [HttpPost("blackjack/{gameId}/stand")]
    [ProducesResponseType(typeof(BlackjackStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BlackjackStateDto>> StandBlackjack(string gameId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _blackjackService.StandAsync(userId, gameId);
            
            _logger.LogInformation(
                "User {UserId} stood in blackjack game {GameId} - Status: {Status}, Win: {WinAmount}", 
                userId, gameId, result.Status, result.WinAmount ?? 0);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Blackjack stand failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized blackjack stand: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during blackjack stand");
            return StatusCode(500, new { message = "An error occurred during stand" });
        }
    }

    [HttpPost("blackjack/{gameId}/double")]
    [ProducesResponseType(typeof(BlackjackStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BlackjackStateDto>> DoubleDownBlackjack(string gameId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _blackjackService.DoubleDownAsync(userId, gameId);
            
            _logger.LogInformation(
                "User {UserId} doubled down in blackjack game {GameId} - Status: {Status}, Win: {WinAmount}", 
                userId, gameId, result.Status, result.WinAmount ?? 0);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Blackjack double down failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized blackjack double down: {Message}", ex.Message);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during blackjack double down");
            return StatusCode(500, new { message = "An error occurred during double down" });
        }
    }

    [HttpPost("roulette/spin")]
    [ProducesResponseType(typeof(RouletteResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RouletteResultDto>> SpinRoulette([FromBody] RouletteSpinDto spinDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _rouletteService.SpinAsync(userId, spinDto.Bets);
            
            _logger.LogInformation(
                "User {UserId} played roulette - Bets: {BetCount}, Total Bet: {TotalBet}, Win: {WinAmount}, Number: {WinningNumber}", 
                userId, spinDto.Bets.Count, spinDto.Bets.Sum(b => b.Amount), result.TotalWinAmount, result.WinningNumber);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Roulette spin failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during roulette spin");
            return StatusCode(500, new { message = "An error occurred during roulette spin" });
        }
    }

    [HttpPost("poker/start")]
    [ProducesResponseType(typeof(PokerStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PokerStateDto>> StartPoker([FromBody] PokerStartDto startDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var result = await _pokerService.StartGameAsync(userId, startDto.BetAmount);
            
            _logger.LogInformation(
                "User {UserId} started poker - Bet: {BetAmount}, GameId: {GameId}", 
                userId, startDto.BetAmount, result.GameId);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Poker start failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting poker");
            return StatusCode(500, new { message = "An error occurred starting poker" });
        }
    }

    [HttpPost("poker/{gameId}/draw")]
    [ProducesResponseType(typeof(PokerStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PokerStateDto>> DrawPoker(string gameId, [FromBody] PokerDrawDto drawDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _pokerService.DrawAsync(gameId, drawDto.CardsToHold);
            
            _logger.LogInformation(
                "Poker game {GameId} draw completed - Hand: {HandRank}, Win: {WinAmount}", 
                gameId, result.HandRank, result.WinAmount);
            
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Poker draw failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during poker draw");
            return StatusCode(500, new { message = "An error occurred during draw" });
        }
    }

    [HttpGet("poker/{gameId}")]
    [ProducesResponseType(typeof(PokerStateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PokerStateDto>> GetPokerGame(string gameId)
    {
        try
        {
            var result = await _pokerService.GetGameStateAsync(gameId);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Get poker game failed: {Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting poker game");
            return StatusCode(500, new { message = "An error occurred getting game state" });
        }
    }
}
