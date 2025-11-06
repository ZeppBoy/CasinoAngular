import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { GameService } from '../../../services/game.service';
import { AuthService } from '../../../services/auth.service';
import { BlackjackState, Card } from '../../../models/game.model';
import { User } from '../../../models/auth.model';

@Component({
  selector: 'app-blackjack',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="blackjack-game">
      <nav class="navbar">
        <div class="nav-brand" routerLink="/dashboard">🃏 Casino</div>
        <div class="nav-items">
          <span class="balance">Balance: \${{ (user?.balance || 0).toFixed(2) }}</span>
          <span class="username">{{ user?.username }}</span>
          <button routerLink="/dashboard" class="back-btn">← Dashboard</button>
        </div>
      </nav>

      <div class="container">
        <h1>🃏 Blackjack</h1>
        
        <div class="game-area">
          <!-- Betting Phase -->
          <div class="betting-phase" *ngIf="!gameState">
            <h2>Place Your Bet</h2>
            <div class="bet-control">
              <label for="betAmount">Bet Amount:</label>
              <input 
                type="number" 
                id="betAmount"
                [(ngModel)]="betAmount"
                min="1"
                max="{{ user?.balance || 0 }}"
                step="1"
              />
            </div>
            
            <div class="quick-bets">
              <button (click)="setBet(10)">$10</button>
              <button (click)="setBet(25)">$25</button>
              <button (click)="setBet(50)">$50</button>
              <button (click)="setBet(100)">$100</button>
            </div>

            <button class="deal-btn" (click)="startGame()" [disabled]="loading">
              {{ loading ? 'Dealing...' : 'DEAL' }}
            </button>
          </div>

          <!-- Game Phase -->
          <div class="game-phase" *ngIf="gameState">
            <!-- Dealer Hand -->
            <div class="hand dealer-hand">
              <h3>Dealer's Hand ({{ gameState.dealerShowingHoleCard ? gameState.dealerHandValue : '?' }})</h3>
              <div class="cards">
                <div class="card" *ngFor="let card of gameState.dealerHand; let i = index">
                  <span *ngIf="gameState.dealerShowingHoleCard || i === 0">
                    {{ getCardDisplay(card) }}
                  </span>
                  <span *ngIf="!gameState.dealerShowingHoleCard && i === 1" class="hole-card">
                    🂠
                  </span>
                </div>
              </div>
            </div>

            <!-- Player Hand -->
            <div class="hand player-hand">
              <h3>Your Hand ({{ gameState.playerHandValue }})</h3>
              <div class="cards">
                <div class="card" *ngFor="let card of gameState.playerHand">
                  {{ getCardDisplay(card) }}
                </div>
              </div>
            </div>

            <!-- Game Status -->
            <div class="game-status" *ngIf="gameState.status !== 'Playing'">
              <div [ngClass]="{
                'status-win': gameState.status === 'PlayerWins' || gameState.status === 'PlayerBlackjack',
                'status-lose': gameState.status === 'DealerWins',
                'status-push': gameState.status === 'Push'
              }">
                <h2>{{ getStatusMessage() }}</h2>
                <p *ngIf="gameState.winAmount && gameState.winAmount > 0" class="win-amount">
                  Won: \${{ gameState.winAmount.toFixed(2) }}
                </p>
              </div>
            </div>

            <!-- Action Buttons -->
            <div class="actions" *ngIf="gameState.status === 'Playing'">
              <button 
                class="action-btn hit-btn"
                (click)="hit()"
                [disabled]="loading || !gameState.canHit"
              >
                HIT
              </button>
              <button 
                class="action-btn stand-btn"
                (click)="stand()"
                [disabled]="loading || !gameState.canStand"
              >
                STAND
              </button>
              <button 
                class="action-btn double-btn"
                (click)="double()"
                [disabled]="loading || !gameState.canDouble"
              >
                DOUBLE
              </button>
            </div>

            <!-- New Game Button -->
            <div class="new-game" *ngIf="gameState.status !== 'Playing'">
              <button class="new-game-btn" (click)="newGame()">
                New Game
              </button>
            </div>
          </div>

          <!-- Error Message -->
          <div class="error" *ngIf="errorMessage">
            {{ errorMessage }}
          </div>
        </div>

        <!-- Game Info -->
        <div class="game-info">
          <h3>How to Play</h3>
          <p><strong>Goal:</strong> Get closer to 21 than the dealer without going over</p>
          <p><strong>Hit:</strong> Take another card</p>
          <p><strong>Stand:</strong> Keep your current hand</p>
          <p><strong>Double:</strong> Double your bet and take one more card</p>
          <p><strong>Blackjack:</strong> Ace + 10-value card pays 1.5x</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .blackjack-game {
      min-height: 100vh;
      background: linear-gradient(135deg, #0f2027 0%, #203a43 50%, #2c5364 100%);
    }

    .navbar {
      background: rgba(255, 255, 255, 0.95);
      padding: 1rem 2rem;
      display: flex;
      justify-content: space-between;
      align-items: center;
      box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
    }

    .nav-brand {
      font-size: 1.5rem;
      font-weight: 700;
      color: #667eea;
      cursor: pointer;
    }

    .nav-items {
      display: flex;
      gap: 1.5rem;
      align-items: center;
    }

    .balance {
      font-weight: 600;
      color: #28a745;
      font-size: 1.1rem;
    }

    .username {
      color: #333;
    }

    .back-btn {
      padding: 0.5rem 1rem;
      background: #6c757d;
      color: white;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      font-weight: 600;
    }

    .container {
      max-width: 1000px;
      margin: 0 auto;
      padding: 2rem;
    }

    h1 {
      text-align: center;
      color: white;
      font-size: 2.5rem;
      margin-bottom: 2rem;
    }

    .game-area {
      background: linear-gradient(145deg, #1e3c72, #2a5298);
      border-radius: 15px;
      padding: 2rem;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
      margin-bottom: 2rem;
    }

    .betting-phase {
      text-align: center;
      padding: 2rem;
    }

    .betting-phase h2 {
      color: white;
      margin-bottom: 2rem;
      font-size: 2rem;
    }

    .bet-control {
      display: flex;
      justify-content: center;
      align-items: center;
      gap: 1rem;
      margin-bottom: 1.5rem;
    }

    .bet-control label {
      font-weight: 600;
      color: white;
      font-size: 1.2rem;
    }

    .bet-control input {
      width: 150px;
      padding: 0.75rem;
      border: 2px solid #fff;
      border-radius: 5px;
      font-size: 1.1rem;
      text-align: center;
      background: rgba(255, 255, 255, 0.9);
    }

    .quick-bets {
      display: flex;
      gap: 1rem;
      justify-content: center;
      margin-bottom: 2rem;
    }

    .quick-bets button {
      padding: 0.75rem 1.5rem;
      background: rgba(255, 255, 255, 0.2);
      color: white;
      border: 2px solid white;
      border-radius: 5px;
      cursor: pointer;
      font-weight: 600;
      font-size: 1.1rem;
      transition: all 0.2s;
    }

    .quick-bets button:hover {
      background: white;
      color: #2a5298;
    }

    .deal-btn, .new-game-btn {
      padding: 1.5rem 4rem;
      background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
      color: white;
      border: none;
      border-radius: 50px;
      font-size: 1.5rem;
      font-weight: 700;
      cursor: pointer;
      transition: transform 0.2s;
      box-shadow: 0 5px 15px rgba(56, 239, 125, 0.4);
    }

    .deal-btn:hover:not(:disabled),
    .new-game-btn:hover {
      transform: translateY(-3px);
      box-shadow: 0 8px 20px rgba(56, 239, 125, 0.6);
    }

    .deal-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .game-phase {
      color: white;
    }

    .hand {
      margin: 2rem 0;
      text-align: center;
    }

    .hand h3 {
      margin-bottom: 1rem;
      font-size: 1.5rem;
    }

    .dealer-hand h3 {
      color: #ff6b6b;
    }

    .player-hand h3 {
      color: #51cf66;
    }

    .cards {
      display: flex;
      justify-content: center;
      gap: 1rem;
      flex-wrap: wrap;
    }

    .card {
      width: 80px;
      height: 110px;
      background: white;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 2.5rem;
      box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
      transition: transform 0.2s;
    }

    .card:hover {
      transform: translateY(-5px);
    }

    .hole-card {
      font-size: 3rem;
      color: #2a5298;
    }

    .game-status {
      margin: 2rem 0;
      text-align: center;
      padding: 2rem;
      border-radius: 10px;
    }

    .status-win {
      background: linear-gradient(135deg, #11998e 0%, #38ef7d 100%);
    }

    .status-lose {
      background: linear-gradient(135deg, #eb3349 0%, #f45c43 100%);
    }

    .status-push {
      background: linear-gradient(135deg, #ffd89b 0%, #19547b 100%);
    }

    .game-status h2 {
      font-size: 2.5rem;
      margin-bottom: 1rem;
    }

    .win-amount {
      font-size: 2rem;
      font-weight: 700;
    }

    .actions {
      display: flex;
      justify-content: center;
      gap: 1rem;
      margin: 2rem 0;
      flex-wrap: wrap;
    }

    .action-btn {
      padding: 1rem 2rem;
      border: none;
      border-radius: 8px;
      font-size: 1.2rem;
      font-weight: 700;
      cursor: pointer;
      transition: all 0.2s;
      min-width: 120px;
    }

    .hit-btn {
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
    }

    .stand-btn {
      background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
      color: white;
    }

    .double-btn {
      background: linear-gradient(135deg, #ffd89b 0%, #19547b 100%);
      color: white;
    }

    .action-btn:hover:not(:disabled) {
      transform: translateY(-3px);
      box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
    }

    .action-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .new-game {
      text-align: center;
      margin-top: 2rem;
    }

    .error {
      background: #dc3545;
      color: white;
      padding: 1rem;
      border-radius: 5px;
      text-align: center;
      margin-top: 1rem;
    }

    .game-info {
      background: white;
      border-radius: 10px;
      padding: 1.5rem;
      box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
    }

    .game-info h3 {
      color: #333;
      margin-bottom: 1rem;
    }

    .game-info p {
      color: #666;
      margin: 0.5rem 0;
    }

    @media (max-width: 768px) {
      .cards {
        gap: 0.5rem;
      }

      .card {
        width: 60px;
        height: 85px;
        font-size: 1.8rem;
      }

      .actions {
        flex-direction: column;
      }

      .action-btn {
        width: 100%;
      }
    }
  `]
})
export class BlackjackComponent implements OnInit {
  user: User | null = null;
  betAmount: number = 10;
  loading: boolean = false;
  gameState: BlackjackState | null = null;
  errorMessage: string = '';

  constructor(
    private gameService: GameService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.user = user;
      if (!user) {
        this.router.navigate(['/login']);
      }
    });
  }

  setBet(amount: number): void {
    if (amount <= (this.user?.balance || 0)) {
      this.betAmount = amount;
    }
  }

  startGame(): void {
    if (this.betAmount <= 0 || this.betAmount > (this.user?.balance || 0)) {
      this.errorMessage = 'Invalid bet amount';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    this.gameService.startBlackjack({ betAmount: this.betAmount }).subscribe({
      next: (state) => {
        this.gameState = state;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to start game.';
        this.loading = false;
      }
    });
  }

  hit(): void {
    if (!this.gameState) return;

    this.loading = true;
    this.gameService.hitBlackjack(this.gameState.gameId).subscribe({
      next: (state) => {
        this.gameState = state;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to hit.';
        this.loading = false;
      }
    });
  }

  stand(): void {
    if (!this.gameState) return;

    this.loading = true;
    this.gameService.standBlackjack(this.gameState.gameId).subscribe({
      next: (state) => {
        this.gameState = state;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to stand.';
        this.loading = false;
      }
    });
  }

  double(): void {
    if (!this.gameState) return;

    this.loading = true;
    this.gameService.doubleBlackjack(this.gameState.gameId).subscribe({
      next: (state) => {
        this.gameState = state;
        this.loading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to double.';
        this.loading = false;
      }
    });
  }

  newGame(): void {
    this.gameState = null;
    this.errorMessage = '';
  }

  getCardDisplay(card: Card): string {
    const suitSymbols: { [key: string]: string } = {
      'Hearts': '♥️',
      'Diamonds': '♦️',
      'Clubs': '♣️',
      'Spades': '♠️'
    };
    return `${card.rank}${suitSymbols[card.suit] || ''}`;
  }

  getStatusMessage(): string {
    if (!this.gameState) return '';
    
    switch (this.gameState.status) {
      case 'PlayerBlackjack':
        return '🎉 BLACKJACK! 🎉';
      case 'PlayerWins':
        return '🎉 YOU WIN! 🎉';
      case 'DealerWins':
        return '😔 Dealer Wins';
      case 'Push':
        return '🤝 Push (Tie)';
      case 'PlayerBusted':
        return '💥 Busted!';
      default:
        return this.gameState.status;
    }
  }
}
