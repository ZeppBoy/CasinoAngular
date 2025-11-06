using System.Security.Cryptography;
using CasinoAPI.Core.DTOs;
using CasinoAPI.Core.Entities;
using CasinoAPI.Core.Interfaces;

namespace CasinoAPI.Core.Services;

public class BlackjackService : IBlackjackService
{
    private readonly IUserRepository _userRepository;
    private readonly ITransactionService _transactionService;
    
    private static readonly Dictionary<string, BlackjackGame> _activeGames = new();
    
    private static readonly string[] Suits = { "♠", "♥", "♦", "♣" };
    private static readonly string[] Ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };
    
    public BlackjackService(IUserRepository userRepository, ITransactionService transactionService)
    {
        _userRepository = userRepository;
        _transactionService = transactionService;
    }

    public async Task<BlackjackStateDto> StartGameAsync(int userId, decimal betAmount)
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
        await _userRepository.UpdateAsync(user);

        // Create bet transaction
        await _transactionService.CreateTransactionAsync(
            userId,
            "Bet",
            betAmount,
            "Blackjack",
            $"Blackjack game - Bet: ${betAmount:F2}"
        );

        // Create new deck and shuffle
        var deck = CreateAndShuffleDeck();

        // Create game
        var game = new BlackjackGame
        {
            UserId = userId,
            BetAmount = betAmount,
            Status = "Playing"
        };

        // Deal initial cards
        game.PlayerCards.Add(deck[0]);
        game.DealerCards.Add(deck[1]);
        game.PlayerCards.Add(deck[2]);
        game.DealerCards.Add(deck[3]);

        _activeGames[game.GameId] = game;

        // Check for blackjacks
        var playerValue = CalculateHandValue(game.PlayerCards);
        var dealerValue = CalculateHandValue(game.DealerCards);

        if (playerValue == 21 && game.PlayerCards.Count == 2)
        {
            game.Status = dealerValue == 21 ? "Push" : "PlayerBlackjack";
            return await FinalizeGameAsync(user, game);
        }

        if (dealerValue == 21 && game.DealerCards.Count == 2)
        {
            game.Status = "DealerBlackjack";
            return await FinalizeGameAsync(user, game);
        }

        return CreateStateDto(game, user.Balance, false);
    }

    public async Task<BlackjackStateDto> HitAsync(int userId, string gameId)
    {
        if (!_activeGames.TryGetValue(gameId, out var game))
        {
            throw new InvalidOperationException("Game not found");
        }

        if (game.UserId != userId)
        {
            throw new UnauthorizedAccessException("Not your game");
        }

        if (game.Status != "Playing")
        {
            throw new InvalidOperationException("Game is not in playing state");
        }

        // Deal card to player
        var deck = CreateAndShuffleDeck();
        game.PlayerCards.Add(deck[0]);

        var playerValue = CalculateHandValue(game.PlayerCards);

        if (playerValue > 21)
        {
            game.Status = "PlayerBust";
            var user = await _userRepository.GetByIdAsync(userId);
            return await FinalizeGameAsync(user!, game);
        }

        return CreateStateDto(game, (await _userRepository.GetByIdAsync(userId))!.Balance, false);
    }

    public async Task<BlackjackStateDto> StandAsync(int userId, string gameId)
    {
        if (!_activeGames.TryGetValue(gameId, out var game))
        {
            throw new InvalidOperationException("Game not found");
        }

        if (game.UserId != userId)
        {
            throw new UnauthorizedAccessException("Not your game");
        }

        if (game.Status != "Playing")
        {
            throw new InvalidOperationException("Game is not in playing state");
        }

        // Dealer plays
        var deck = CreateAndShuffleDeck();
        var deckIndex = 0;
        
        while (CalculateHandValue(game.DealerCards) < 17)
        {
            game.DealerCards.Add(deck[deckIndex++]);
        }

        var dealerValue = CalculateHandValue(game.DealerCards);
        var playerValue = CalculateHandValue(game.PlayerCards);

        if (dealerValue > 21)
        {
            game.Status = "DealerBust";
        }
        else if (playerValue > dealerValue)
        {
            game.Status = "PlayerWin";
        }
        else if (dealerValue > playerValue)
        {
            game.Status = "DealerWin";
        }
        else
        {
            game.Status = "Push";
        }

        var user = await _userRepository.GetByIdAsync(userId);
        return await FinalizeGameAsync(user!, game);
    }

    public async Task<BlackjackStateDto> DoubleDownAsync(int userId, string gameId)
    {
        if (!_activeGames.TryGetValue(gameId, out var game))
        {
            throw new InvalidOperationException("Game not found");
        }

        if (game.UserId != userId)
        {
            throw new UnauthorizedAccessException("Not your game");
        }

        if (game.Status != "Playing")
        {
            throw new InvalidOperationException("Game is not in playing state");
        }

        if (game.PlayerCards.Count != 2)
        {
            throw new InvalidOperationException("Can only double down on initial hand");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        if (user.Balance < game.BetAmount)
        {
            throw new InvalidOperationException("Insufficient balance to double down");
        }

        // Deduct additional bet amount
        user.Balance -= game.BetAmount;
        await _userRepository.UpdateAsync(user);

        // Create additional bet transaction
        await _transactionService.CreateTransactionAsync(
            userId,
            "Bet",
            game.BetAmount,
            "Blackjack",
            $"Blackjack double down - Bet: ${game.BetAmount:F2}"
        );

        // Double the bet
        game.BetAmount *= 2;

        // Deal one card
        var deck = CreateAndShuffleDeck();
        game.PlayerCards.Add(deck[0]);

        var playerValue = CalculateHandValue(game.PlayerCards);

        if (playerValue > 21)
        {
            game.Status = "PlayerBust";
            return await FinalizeGameAsync(user, game);
        }

        // Dealer plays
        var deckIndex = 1;
        while (CalculateHandValue(game.DealerCards) < 17)
        {
            game.DealerCards.Add(deck[deckIndex++]);
        }

        var dealerValue = CalculateHandValue(game.DealerCards);

        if (dealerValue > 21)
        {
            game.Status = "DealerBust";
        }
        else if (playerValue > dealerValue)
        {
            game.Status = "PlayerWin";
        }
        else if (dealerValue > playerValue)
        {
            game.Status = "DealerWin";
        }
        else
        {
            game.Status = "Push";
        }

        return await FinalizeGameAsync(user, game);
    }

    private async Task<BlackjackStateDto> FinalizeGameAsync(User user, BlackjackGame game)
    {
        decimal winAmount = 0;

        switch (game.Status)
        {
            case "PlayerBlackjack":
                winAmount = game.BetAmount * 2.5m; // Blackjack pays 3:2
                break;
            case "PlayerWin":
            case "DealerBust":
                winAmount = game.BetAmount * 2; // Regular win pays 1:1
                break;
            case "Push":
                winAmount = game.BetAmount; // Return bet
                break;
            case "DealerWin":
            case "PlayerBust":
            case "DealerBlackjack":
                winAmount = 0; // Player loses
                break;
        }

        if (winAmount > 0)
        {
            user.Balance += winAmount;
            await _userRepository.UpdateAsync(user);

            var description = game.Status == "PlayerBlackjack"
                ? $"BLACKJACK! Win - ${winAmount:F2}"
                : game.Status == "Push"
                    ? $"Blackjack push - Bet returned ${winAmount:F2}"
                    : $"Blackjack win - ${winAmount:F2}";

            await _transactionService.CreateTransactionAsync(
                game.UserId,
                "Win",
                winAmount,
                "Blackjack",
                description
            );
        }

        _activeGames.Remove(game.GameId);

        return CreateStateDto(game, user.Balance, true, winAmount);
    }

    private BlackjackStateDto CreateStateDto(BlackjackGame game, decimal balance, bool showDealerHoleCard, decimal? winAmount = null)
    {
        var dealerCards = ParseCards(game.DealerCards);
        var playerCards = ParseCards(game.PlayerCards);

        var dealerHandValue = CalculateHandValue(game.DealerCards);
        var playerHandValue = CalculateHandValue(game.PlayerCards);

        // Hide dealer hole card if game is still playing
        if (!showDealerHoleCard && game.Status == "Playing")
        {
            dealerCards = new List<CardDto> { dealerCards[0] };
            dealerHandValue = GetCardValue(game.DealerCards[0]);
        }

        return new BlackjackStateDto
        {
            GameId = game.GameId,
            PlayerHand = playerCards,
            DealerHand = dealerCards,
            PlayerHandValue = playerHandValue,
            DealerHandValue = dealerHandValue,
            DealerShowingHoleCard = showDealerHoleCard,
            Status = game.Status,
            WinAmount = winAmount,
            BalanceAfter = balance,
            CanHit = game.Status == "Playing",
            CanStand = game.Status == "Playing",
            CanDouble = game.Status == "Playing" && game.PlayerCards.Count == 2
        };
    }

    private List<string> CreateAndShuffleDeck()
    {
        var deck = new List<string>();
        
        foreach (var suit in Suits)
        {
            foreach (var rank in Ranks)
            {
                deck.Add($"{rank}{suit}");
            }
        }

        // Fisher-Yates shuffle with cryptographically secure RNG
        for (int i = deck.Count - 1; i > 0; i--)
        {
            var randomBytes = new byte[4];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var j = (int)(BitConverter.ToUInt32(randomBytes, 0) % (i + 1));
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }

        return deck;
    }

    private int CalculateHandValue(List<string> cards)
    {
        int value = 0;
        int aces = 0;

        foreach (var card in cards)
        {
            var rank = card[..^1];
            
            if (rank == "A")
            {
                aces++;
                value += 11;
            }
            else if (rank == "J" || rank == "Q" || rank == "K")
            {
                value += 10;
            }
            else
            {
                value += int.Parse(rank);
            }
        }

        while (value > 21 && aces > 0)
        {
            value -= 10;
            aces--;
        }

        return value;
    }

    private int GetCardValue(string card)
    {
        var rank = card[..^1];
        
        if (rank == "A")
            return 11;
        if (rank == "J" || rank == "Q" || rank == "K")
            return 10;
        return int.Parse(rank);
    }

    private List<CardDto> ParseCards(List<string> cards)
    {
        return cards.Select(card => new CardDto
        {
            Suit = card[^1..],
            Rank = card[..^1],
            Value = GetCardValue(card)
        }).ToList();
    }
}
