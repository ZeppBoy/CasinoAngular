import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { GameService } from '../../../services/game.service';
import { AuthService } from '../../../services/auth.service';
import { PokerState } from '../../../models/game.model';
import { User } from '../../../models/auth.model';

@Component({
  selector: 'app-poker',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="poker-game">
      <nav class="navbar">
        <div class="nav-brand" routerLink="/dashboard">🎰 Casino</div>
        <div class="nav-items">
          <span class="balance">Balance: \${{ (user?.balance || 0).toFixed(2) }}</span>
          <span class="username">{{ user?.username }}</span>
          <button routerLink="/dashboard" class="back-btn">← Dashboard</button>
        </div>
      </nav>

      <div class="container">
        <h1>🃏 Video Poker</h1>
        <p class="subtitle">5-Card Draw - Jacks or Better</p>
        
        <div class="game-area">
          <div class="poker-hand" *ngIf="currentGame">
            <div class="cards">
              <div 
                *ngFor="let card of currentGame.hand; let i = index" 
                class="card-wrapper"
                (click)="toggleCard(i)">
                <div class="card" [class.selected]="selectedCards.includes(i)">
                  <div class="card-rank">{{ card.rank }}</div>
                  <div class="card-suit" [class.red]="isRedSuit(card.suit)">
                    {{ getSuitSymbol(card.suit) }}
                  </div>
                </div>
                <div class="hold-label" *ngIf="selectedCards.includes(i) && currentGame.canDraw">
                  HOLD
                </div>
              </div>
            </div>
          </div>

          <div class="hand-rank" *ngIf="currentGame">
            <h3>{{ currentGame.handRank }}</h3>
            <div class="payout-info" *ngIf="currentGame.payout > 0">
              Payout: {{ currentGame.payout }}x
            </div>
          </div>

          <div class="win-display" *ngIf="currentGame && currentGame.winAmount > 0">
            <h2 class="win-amount">🎉 Win: \${{ currentGame.winAmount.toFixed(2) }}</h2>
          </div>

          <div class="loss-display" *ngIf="currentGame && currentGame.status === 'Lost' && !currentGame.canDraw">
            <p>Better luck next time!</p>
          </div>

          <div class="controls" *ngIf="!isPlaying">
            <div class="bet-section">
              <label for="betAmount">Bet Amount:</label>
              <input 
                type="number" 
                id="betAmount" 
                [(ngModel)]="betAmount" 
                min="0.50" 
                max="1000" 
                step="0.50">
              <div class="bet-presets">
                <button (click)="setBet(1)">$1</button>
                <button (click)="setBet(5)">$5</button>
                <button (click)="setBet(10)">$10</button>
                <button (click)="setBet(25)">$25</button>
              </div>
            </div>

            <button class="deal-btn" (click)="dealHand()">
              Deal Hand
            </button>
          </div>

          <div class="draw-controls" *ngIf="isPlaying && currentGame?.canDraw">
            <p class="instruction">Click cards to hold, then click Draw</p>
            <button class="draw-btn" (click)="draw()">Draw</button>
          </div>

          <div class="new-game" *ngIf="isPlaying && !currentGame?.canDraw">
            <button class="new-game-btn" (click)="newGame()">New Game</button>
          </div>

          <div class="error" *ngIf="errorMessage">{{ errorMessage }}</div>

          <div class="payout-table">
            <h3>Payout Table</h3>
            <table>
              <tr><td>Royal Flush</td><td>250x</td></tr>
              <tr><td>Straight Flush</td><td>50x</td></tr>
              <tr><td>Four of a Kind</td><td>25x</td></tr>
              <tr><td>Full House</td><td>9x</td></tr>
              <tr><td>Flush</td><td>6x</td></tr>
              <tr><td>Straight</td><td>4x</td></tr>
              <tr><td>Three of a Kind</td><td>3x</td></tr>
              <tr><td>Two Pair</td><td>2x</td></tr>
              <tr><td>Jacks or Better</td><td>1x</td></tr>
            </table>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .poker-game { min-height: 100vh; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding-bottom: 2rem; }
    .navbar { background: rgba(255, 255, 255, 0.1); backdrop-filter: blur(10px); padding: 1rem 2rem; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid rgba(255, 255, 255, 0.2); }
    .nav-brand { font-size: 1.5rem; font-weight: bold; color: white; cursor: pointer; }
    .nav-items { display: flex; gap: 2rem; align-items: center; color: white; }
    .balance { font-weight: 600; font-size: 1.1rem; }
    .back-btn { background: rgba(255, 255, 255, 0.2); color: white; border: none; padding: 0.5rem 1rem; border-radius: 8px; cursor: pointer; transition: all 0.3s; }
    .container { max-width: 1200px; margin: 0 auto; padding: 2rem; }
    h1 { text-align: center; color: white; margin-bottom: 0.5rem; font-size: 2.5rem; text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3); }
    .subtitle { text-align: center; color: rgba(255, 255, 255, 0.9); margin-bottom: 2rem; font-size: 1.2rem; }
    .game-area { background: white; border-radius: 16px; padding: 2rem; box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2); }
    .cards { display: flex; justify-content: center; gap: 1rem; flex-wrap: wrap; }
    .card-wrapper { cursor: pointer; }
    .card { width: 100px; height: 140px; background: white; border: 2px solid #333; border-radius: 8px; display: flex; flex-direction: column; align-items: center; justify-content: space-around; padding: 0.5rem; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2); transition: all 0.2s; }
    .card.selected { border-color: #4CAF50; background: #f0fff4; transform: translateY(-10px); }
    .card-rank { font-size: 2rem; font-weight: bold; }
    .card-suit { font-size: 3rem; }
    .card-suit.red { color: #d32f2f; }
    .hold-label { text-align: center; color: #4CAF50; font-weight: bold; margin-top: 0.5rem; }
    .hand-rank { text-align: center; margin: 2rem 0; }
    .hand-rank h3 { color: #667eea; font-size: 1.8rem; }
    .payout-info { color: #4CAF50; font-size: 1.2rem; font-weight: 600; }
    .win-display { text-align: center; margin: 2rem 0; }
    .win-amount { color: #4CAF50; font-size: 2rem; animation: pulse 1s ease-in-out; }
    @keyframes pulse { 0%, 100% { transform: scale(1); } 50% { transform: scale(1.1); } }
    .controls { text-align: center; margin-top: 2rem; }
    .bet-section { margin-bottom: 1.5rem; }
    .bet-section input { width: 150px; padding: 0.75rem; font-size: 1.1rem; border: 2px solid #ddd; border-radius: 8px; text-align: center; }
    .bet-presets { display: flex; gap: 0.5rem; justify-content: center; margin-top: 1rem; }
    .bet-presets button { padding: 0.5rem 1rem; background: #f0f0f0; border: 1px solid #ddd; border-radius: 6px; cursor: pointer; }
    .deal-btn, .draw-btn, .new-game-btn { padding: 1rem 3rem; font-size: 1.2rem; font-weight: bold; color: white; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); border: none; border-radius: 12px; cursor: pointer; transition: all 0.3s; }
    .draw-controls { text-align: center; margin-top: 2rem; }
    .instruction { color: #666; margin-bottom: 1rem; font-size: 1.1rem; }
    .payout-table { margin-top: 3rem; padding-top: 2rem; border-top: 2px solid #eee; }
    .payout-table h3 { text-align: center; color: #333; margin-bottom: 1rem; }
    .payout-table table { width: 100%; max-width: 400px; margin: 0 auto; border-collapse: collapse; }
    .payout-table td { padding: 0.5rem 1rem; border-bottom: 1px solid #eee; }
    .payout-table td:first-child { text-align: left; color: #333; }
    .payout-table td:last-child { text-align: right; font-weight: bold; color: #667eea; }
    .error { text-align: center; color: #d32f2f; margin-top: 1rem; padding: 1rem; background: #ffebee; border-radius: 8px; }
  `]
})
export class PokerComponent implements OnInit {
  user: User | null = null;
  betAmount: number = 5;
  isPlaying: boolean = false;
  errorMessage: string = '';
  currentGame: PokerState | null = null;
  selectedCards: number[] = [];

  constructor(
    private gameService: GameService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.user = user;
      if (!user) this.router.navigate(['/login']);
    });
  }

  setBet(amount: number): void {
    this.betAmount = amount;
  }

  dealHand(): void {
    this.errorMessage = '';
    this.selectedCards = [];
    this.gameService.startPoker({ betAmount: this.betAmount }).subscribe({
      next: (result) => {
        this.currentGame = result;
        this.isPlaying = true;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to start poker game';
      }
    });
  }

  toggleCard(index: number): void {
    if (!this.currentGame?.canDraw) return;
    const cardIndex = this.selectedCards.indexOf(index);
    if (cardIndex > -1) {
      this.selectedCards.splice(cardIndex, 1);
    } else {
      this.selectedCards.push(index);
    }
  }

  draw(): void {
    if (!this.currentGame) return;
    this.gameService.drawPoker(this.currentGame.gameId, { cardsToHold: this.selectedCards }).subscribe({
      next: (result) => {
        this.currentGame = result;
        this.selectedCards = [];
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to draw cards';
      }
    });
  }

  newGame(): void {
    this.isPlaying = false;
    this.currentGame = null;
    this.selectedCards = [];
    this.errorMessage = '';
  }

  getSuitSymbol(suit: string): string {
    const symbols: { [key: string]: string} = { 'Hearts': '♥', 'Diamonds': '♦', 'Clubs': '♣', 'Spades': '♠' };
    return symbols[suit] || suit;
  }

  isRedSuit(suit: string): boolean {
    return suit === 'Hearts' || suit === 'Diamonds';
  }
}
