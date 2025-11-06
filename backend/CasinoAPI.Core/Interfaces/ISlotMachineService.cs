using CasinoAPI.Core.DTOs;

namespace CasinoAPI.Core.Interfaces;

public interface ISlotMachineService
{
    Task<SlotResultDto> SpinAsync(int userId, decimal betAmount);
}
