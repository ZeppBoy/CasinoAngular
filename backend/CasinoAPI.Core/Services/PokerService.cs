using System.Security.Cryptography;
using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CasinoAPI.Core.Services;

public class PokerService : IPokerService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<PokerService> _logger;
    private static readonly Dictionary<string, PokerGame> _activeGames = new();

    public PokerService(
        IUserRepository userRepository,
        ITransactionService transactionService,
        ILogger<PokerService> logger)
    {
        _userRepository = userRepository;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<PokerStateDto> StartGameAsync(int userId, decimal betAmount)
    {
        _logger.LogInformation("Starting poker game for user {UserId} with bet {BetAmount}", userId, betAmount);

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
        await _userRepository.UpdateAsync(user);

        // Create bet transaction
        await _transactionService.CreateTransactionAsync(
            userId,
            "Bet",
            betAmount,
            "Poker",
            $"Poker bet: ${betAmount:F2}"
        );

        // Create new game
        var game = new PokerGame
        {
            GameId = Guid.NewGuid().ToString(),
            UserId = userId,
            BetAmount = betAmount,
            Deck = CreateShuffledDeck(),
            Status = "Playing",
            HasDrawn = false
        };

        // Deal 5 cards
        game.Hand = game.Deck.Take(5).ToList();
        game.Deck = game.Deck.Skip(5).ToList();

        _activeGames[game.GameId] = game;

        return new PokerStateDto
        {
            GameId = game.GameId,
            Hand = game.Hand.Select(c => new CardDto
            {
                Suit = c.Suit,
                Rank = c.Rank,
                Value = c.Value
            }).ToList(),
            HandRank = "Initial Hand",
            BetAmount = betAmount,
            WinAmount = 0,
            Payout = 0,
            BalanceAfter = user.Balance,
            Status = "Playing",
            CanDraw = true,
            CardsToHold = new List<int>()
        };
    }

    public async Task<PokerStateDto> DrawAsync(string gameId, List<int> cardsToHold)
    {
        if (!_activeGames.TryGetValue(gameId, out var game))
        {
            throw new InvalidOperationException("Game not found");
        }

        if (game.HasDrawn)
        {
            throw new InvalidOperationException("Already drawn cards");
        }

        if (game.Status != "Playing")
        {
            throw new InvalidOperationException("Game is not in playing state");
        }

        _logger.LogInformation("Drawing cards for game {GameId}, holding cards at positions: {Cards}", 
            gameId, string.Join(",", cardsToHold));

        // Validate card positions
        if (cardsToHold.Any(pos => pos < 0 || pos >= 5))
        {
            throw new InvalidOperationException("Invalid card positions");
        }

        // Replace cards not being held
        for (int i = 0; i < 5; i++)
        {
            if (!cardsToHold.Contains(i))
            {
                game.Hand[i] = game.Deck.First();
                game.Deck.RemoveAt(0);
            }
        }

        game.HasDrawn = true;

        // Evaluate final hand
        var (handRank, payout) = EvaluateHand(game.Hand);
        game.HandRank = handRank;

        var user = await _userRepository.GetByIdAsync(game.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        decimal winAmount = game.BetAmount * payout;
        game.WinAmount = winAmount;

        if (winAmount > 0)
        {
            user.Balance += winAmount;
            await _userRepository.UpdateAsync(user);

            await _transactionService.CreateTransactionAsync(
                game.UserId,
                "Win",
                winAmount,
                "Poker",
                $"Poker win: {handRank} - ${winAmount:F2}"
            );

            game.Status = "Won";
        }
        else
        {
            game.Status = "Lost";
        }

        _logger.LogInformation("Poker game {GameId} completed. Hand: {HandRank}, Win: ${WinAmount}", 
            gameId, handRank, winAmount);

        return new PokerStateDto
        {
            GameId = game.GameId,
            Hand = game.Hand.Select(c => new CardDto
            {
                Suit = c.Suit,
                Rank = c.Rank,
                Value = c.Value
            }).ToList(),
            HandRank = handRank,
            BetAmount = game.BetAmount,
            WinAmount = winAmount,
            Payout = payout,
            BalanceAfter = user.Balance,
            Status = game.Status,
            CanDraw = false,
            CardsToHold = cardsToHold
        };
    }

    public async Task<PokerStateDto> GetGameStateAsync(string gameId)
    {
        if (!_activeGames.TryGetValue(gameId, out var game))
        {
            throw new InvalidOperationException("Game not found");
        }

        var user = await _userRepository.GetByIdAsync(game.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        return new PokerStateDto
        {
            GameId = game.GameId,
            Hand = game.Hand.Select(c => new CardDto
            {
                Suit = c.Suit,
                Rank = c.Rank,
                Value = c.Value
            }).ToList(),
            HandRank = game.HandRank,
            BetAmount = game.BetAmount,
            WinAmount = game.WinAmount,
            Payout = game.WinAmount > 0 ? game.WinAmount / game.BetAmount : 0,
            BalanceAfter = user.Balance,
            Status = game.Status,
            CanDraw = !game.HasDrawn && game.Status == "Playing",
            CardsToHold = new List<int>()
        };
    }

    private List<Card> CreateShuffledDeck()
    {
        var suits = new[] { "Hearts", "Diamonds", "Clubs", "Spades" };
        var ranks = new[] { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
        var deck = new List<Card>();

        foreach (var suit in suits)
        {
            for (int i = 0; i < ranks.Length; i++)
            {
                deck.Add(new Card
                {
                    Suit = suit,
                    Rank = ranks[i],
                    Value = i + 2 // 2-14
                });
            }
        }

        return ShuffleDeck(deck);
    }

    private List<Card> ShuffleDeck(List<Card> deck)
    {
        var shuffled = new List<Card>(deck);
        using var rng = RandomNumberGenerator.Create();
        int n = shuffled.Count;

        while (n > 1)
        {
            byte[] box = new byte[4];
            rng.GetBytes(box);
            int k = (int)(BitConverter.ToUInt32(box, 0) % n);
            n--;
            (shuffled[k], shuffled[n]) = (shuffled[n], shuffled[k]);
        }

        return shuffled;
    }

    private (string handRank, decimal payout) EvaluateHand(List<Card> hand)
    {
        // Sort hand by value
        var sortedHand = hand.OrderBy(c => c.Value).ToList();
        
        var isFlush = hand.All(c => c.Suit == hand[0].Suit);
        var isStraight = IsStraight(sortedHand);
        var valueCounts = hand.GroupBy(c => c.Value)
                             .ToDictionary(g => g.Key, g => g.Count());
        
        var counts = valueCounts.Values.OrderByDescending(v => v).ToList();
        var uniqueValues = valueCounts.Keys.OrderByDescending(v => v).ToList();

        // Check for Royal Flush
        if (isFlush && isStraight && sortedHand[0].Value == 10)
        {
            return ("Royal Flush", 250m);
        }

        // Straight Flush
        if (isFlush && isStraight)
        {
            return ("Straight Flush", 50m);
        }

        // Four of a Kind
        if (counts[0] == 4)
        {
            return ("Four of a Kind", 25m);
        }

        // Full House
        if (counts[0] == 3 && counts[1] == 2)
        {
            return ("Full House", 9m);
        }

        // Flush
        if (isFlush)
        {
            return ("Flush", 6m);
        }

        // Straight
        if (isStraight)
        {
            return ("Straight", 4m);
        }

        // Three of a Kind
        if (counts[0] == 3)
        {
            return ("Three of a Kind", 3m);
        }

        // Two Pair
        if (counts[0] == 2 && counts[1] == 2)
        {
            return ("Two Pair", 2m);
        }

        // Jacks or Better (Pair of J, Q, K, or A)
        if (counts[0] == 2)
        {
            var pairValue = uniqueValues.First(v => valueCounts[v] == 2);
            if (pairValue >= 11) // J=11, Q=12, K=13, A=14
            {
                return ("Jacks or Better", 1m);
            }
        }

        // No winning hand
        return ("High Card", 0m);
    }

    private bool IsStraight(List<Card> sortedHand)
    {
        // Check regular straight
        bool isRegularStraight = true;
        for (int i = 1; i < sortedHand.Count; i++)
        {
            if (sortedHand[i].Value != sortedHand[i - 1].Value + 1)
            {
                isRegularStraight = false;
                break;
            }
        }

        if (isRegularStraight)
        {
            return true;
        }

        // Check for A-2-3-4-5 (wheel)
        if (sortedHand[0].Value == 2 && sortedHand[1].Value == 3 &&
            sortedHand[2].Value == 4 && sortedHand[3].Value == 5 &&
            sortedHand[4].Value == 14) // Ace
        {
            return true;
        }

        return false;
    }
}
