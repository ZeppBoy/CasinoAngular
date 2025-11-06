using CasinoAPI.Core.DTOs;

namespace CasinoAPI.Core.Interfaces;

public interface IRouletteService
{
    Task<RouletteResultDto> SpinAsync(int userId, List<RouletteBetDto> bets);
}
