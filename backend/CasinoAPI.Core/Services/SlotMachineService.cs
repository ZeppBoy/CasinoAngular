using System.Security.Cryptography;
using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Interfaces;

namespace CasinoAPI.Core.Services;

public class SlotMachineService : ISlotMachineService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionService _transactionService;

    private static readonly string[] Symbols = { "🍒", "🍋", "🍊", "🍇", "🔔", "⭐", "💎" };
    
    private static readonly Dictionary<string, decimal> SymbolPayouts = new()
    {
        { "💎", 100m },  // Diamond - Jackpot
        { "⭐", 50m },   // Star
        { "🔔", 25m },   // Bell
        { "🍇", 15m },   // Grape
        { "🍊", 10m },   // Orange
        { "🍋", 5m },    // Lemon
        { "🍒", 3m }     // Cherry
    };

    private const int ReelCount = 3;
    private const int RowCount = 3;

    public SlotMachineService(IUserRepository userRepository, ITransactionService transactionService)
    {
        _userRepository = userRepository;
        _transactionService = transactionService;
    }

    public async Task<SlotResultDto> SpinAsync(int userId, decimal betAmount)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Balance < betAmount)
        {
            throw new InvalidOperationException("Insufficient balance");
        }

        // Deduct bet amount
        user.Balance -= betAmount;

        // Generate reels
        var reels = GenerateReels();

        // Calculate winnings
        var (winAmount, winLines, isJackpot) = CalculateWinnings(reels, betAmount);

        // Add winnings to balance
        user.Balance += winAmount;

        // Update user
        await _userRepository.UpdateAsync(user);

        // Create bet transaction
        await _transactionService.CreateTransactionAsync(
            userId,
            "Bet",
            betAmount,
            "SlotMachine",
            $"Slot machine spin - Bet: ${betAmount:F2}"
        );

        // Create win transaction if there are winnings
        if (winAmount > 0)
        {
            var description = isJackpot 
                ? $"JACKPOT! Slot machine win - ${winAmount:F2}" 
                : $"Slot machine win - ${winAmount:F2}";

            await _transactionService.CreateTransactionAsync(
                userId,
                "Win",
                winAmount,
                "SlotMachine",
                description
            );
        }

        return new SlotResultDto
        {
            Reels = reels,
            WinAmount = winAmount,
            WinLines = winLines,
            BalanceAfter = user.Balance,
            IsJackpot = isJackpot
        };
    }

    private string[][] GenerateReels()
    {
        var reels = new string[ReelCount][];
        
        for (int i = 0; i < ReelCount; i++)
        {
            reels[i] = new string[RowCount];
            for (int j = 0; j < RowCount; j++)
            {
                reels[i][j] = GetRandomSymbol();
            }
        }
        
        return reels;
    }

    private string GetRandomSymbol()
    {
        var randomBytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        var randomValue = BitConverter.ToUInt32(randomBytes, 0);
        var index = randomValue % Symbols.Length;
        
        return Symbols[index];
    }

    private (decimal winAmount, List<WinLineDto> winLines, bool isJackpot) CalculateWinnings(string[][] reels, decimal betAmount)
    {
        var winLines = new List<WinLineDto>();
        decimal totalWinAmount = 0;
        bool isJackpot = false;

        // Check horizontal lines (3 lines)
        for (int row = 0; row < RowCount; row++)
        {
            var symbol = reels[0][row];
            var count = 1;

            for (int col = 1; col < ReelCount; col++)
            {
                if (reels[col][row] == symbol)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }

            if (count == ReelCount)
            {
                var payout = SymbolPayouts[symbol] * betAmount;
                totalWinAmount += payout;

                if (symbol == "💎")
                {
                    isJackpot = true;
                }

                winLines.Add(new WinLineDto
                {
                    LineNumber = row + 1,
                    Symbol = symbol,
                    Count = count,
                    Payout = payout
                });
            }
        }

        // Check diagonal line (top-left to bottom-right)
        if (reels[0][0] == reels[1][1] && reels[1][1] == reels[2][2])
        {
            var symbol = reels[0][0];
            var payout = SymbolPayouts[symbol] * betAmount;
            totalWinAmount += payout;

            if (symbol == "💎")
            {
                isJackpot = true;
            }

            winLines.Add(new WinLineDto
            {
                LineNumber = 4,
                Symbol = symbol,
                Count = 3,
                Payout = payout
            });
        }

        // Check diagonal line (bottom-left to top-right)
        if (reels[0][2] == reels[1][1] && reels[1][1] == reels[2][0])
        {
            var symbol = reels[0][2];
            var payout = SymbolPayouts[symbol] * betAmount;
            totalWinAmount += payout;

            if (symbol == "💎")
            {
                isJackpot = true;
            }

            winLines.Add(new WinLineDto
            {
                LineNumber = 5,
                Symbol = symbol,
                Count = 3,
                Payout = payout
            });
        }

        return (totalWinAmount, winLines, isJackpot);
    }
}
