using System.Security.Cryptography;
using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Interfaces;

namespace CasinoAPI.Core.Services;

public class RouletteService : IRouletteService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionService _transactionService;

    private static readonly int[] RedNumbers = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };
    private static readonly int[] BlackNumbers = { 2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35 };

    public RouletteService(IUserRepository userRepository, ITransactionService transactionService)
    {
        _userRepository = userRepository;
        _transactionService = transactionService;
    }

    public async Task<RouletteResultDto> SpinAsync(int userId, List<RouletteBetDto> bets)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        // Calculate total bet amount
        var totalBetAmount = bets.Sum(b => b.Amount);

        if (user.Balance < totalBetAmount)
        {
            throw new InvalidOperationException("Insufficient balance");
        }

        // Deduct total bet amount
        user.Balance -= totalBetAmount;
        await _userRepository.UpdateAsync(user);

        // Create bet transaction
        await _transactionService.CreateTransactionAsync(
            userId,
            "Bet",
            totalBetAmount,
            "Roulette",
            $"Roulette spin - Total bet: ${totalBetAmount:F2} ({bets.Count} bet{(bets.Count > 1 ? "s" : "")})"
        );

        // Spin the wheel
        var winningNumber = SpinWheel();

        // Determine winning number properties
        var color = GetColor(winningNumber);
        var isEven = winningNumber != 0 && winningNumber % 2 == 0;
        var isHigh = winningNumber >= 19 && winningNumber <= 36;

        // Evaluate each bet
        var betResults = new List<RouletteBetResultDto>();
        decimal totalWinAmount = 0;

        foreach (var bet in bets)
        {
            var (isWin, payout) = EvaluateBet(bet, winningNumber, color, isEven, isHigh);
            var winAmount = isWin ? bet.Amount * payout : 0;
            totalWinAmount += winAmount;

            betResults.Add(new RouletteBetResultDto
            {
                BetType = bet.BetType,
                BetAmount = bet.Amount,
                IsWin = isWin,
                WinAmount = winAmount,
                Payout = payout
            });
        }

        // Add winnings to balance
        if (totalWinAmount > 0)
        {
            user.Balance += totalWinAmount;
            await _userRepository.UpdateAsync(user);

            // Create win transaction
            await _transactionService.CreateTransactionAsync(
                userId,
                "Win",
                totalWinAmount,
                "Roulette",
                $"Roulette win - Number: {winningNumber} ({color}) - ${totalWinAmount:F2}"
            );
        }

        return new RouletteResultDto
        {
            WinningNumber = winningNumber,
            Color = color,
            IsEven = isEven,
            IsHigh = isHigh,
            BetResults = betResults,
            TotalWinAmount = totalWinAmount,
            BalanceAfter = user.Balance
        };
    }

    private int SpinWheel()
    {
        var randomBytes = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        
        var randomValue = BitConverter.ToUInt32(randomBytes, 0);
        return (int)(randomValue % 37); // 0-36
    }

    private string GetColor(int number)
    {
        if (number == 0)
            return "Green";
        if (RedNumbers.Contains(number))
            return "Red";
        return "Black";
    }

    private (bool isWin, decimal payout) EvaluateBet(RouletteBetDto bet, int winningNumber, string color, bool isEven, bool isHigh)
    {
        switch (bet.BetType.ToLower())
        {
            case "number":
            case "straight":
                // Straight-up bet on a single number (35:1)
                return (bet.Number == winningNumber, 36m);

            case "red":
                // Bet on red (1:1)
                return (color == "Red", 2m);

            case "black":
                // Bet on black (1:1)
                return (color == "Black", 2m);

            case "even":
                // Bet on even numbers (1:1)
                return (winningNumber != 0 && isEven, 2m);

            case "odd":
                // Bet on odd numbers (1:1)
                return (winningNumber != 0 && !isEven, 2m);

            case "high":
                // Bet on high numbers 19-36 (1:1)
                return (isHigh, 2m);

            case "low":
                // Bet on low numbers 1-18 (1:1)
                return (winningNumber >= 1 && winningNumber <= 18, 2m);

            case "dozen":
                // Bet on dozens (2:1)
                if (bet.Range == "1st" || bet.Range == "first")
                    return (winningNumber >= 1 && winningNumber <= 12, 3m);
                if (bet.Range == "2nd" || bet.Range == "second")
                    return (winningNumber >= 13 && winningNumber <= 24, 3m);
                if (bet.Range == "3rd" || bet.Range == "third")
                    return (winningNumber >= 25 && winningNumber <= 36, 3m);
                break;

            case "column":
                // Bet on columns (2:1)
                if (bet.Range == "1st" || bet.Range == "first")
                    return (winningNumber > 0 && (winningNumber - 1) % 3 == 0, 3m);
                if (bet.Range == "2nd" || bet.Range == "second")
                    return (winningNumber > 0 && (winningNumber - 2) % 3 == 0, 3m);
                if (bet.Range == "3rd" || bet.Range == "third")
                    return (winningNumber > 0 && winningNumber % 3 == 0, 3m);
                break;
        }

        return (false, 0m);
    }
}
