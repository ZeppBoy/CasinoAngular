using CasinoAPI.Core.DTOs;

namespace CasinoAPI.Core.Interfaces;

public interface IPokerService
{
    Task<PokerStateDto> StartGameAsync(int userId, decimal betAmount);
    Task<PokerStateDto> DrawAsync(string gameId, List<int> cardsToHold);
    Task<PokerStateDto> GetGameStateAsync(string gameId);
}
