using CasinoAPI.Core.DTOs;

namespace CasinoAPI.Core.Interfaces;

public interface IBlackjackService
{
    Task<BlackjackStateDto> StartGameAsync(int userId, decimal betAmount);
    Task<BlackjackStateDto> HitAsync(int userId, string gameId);
    Task<BlackjackStateDto> StandAsync(int userId, string gameId);
    Task<BlackjackStateDto> DoubleDownAsync(int userId, string gameId);
}
