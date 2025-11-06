import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { GameService } from '../../../services/game.service';
import { AuthService } from '../../../services/auth.service';
import { RouletteBet, RouletteResult } from '../../../models/game.model';
import { User } from '../../../models/auth.model';

@Component({
  selector: 'app-roulette',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="roulette-game">
      <nav class="navbar">
        <div class="nav-brand" routerLink="/dashboard">🎲 Casino</div>
        <div class="nav-items">
          <span class="balance">Balance: \${{ (user?.balance || 0).toFixed(2) }}</span>
          <span class="username">{{ user?.username }}</span>
          <button routerLink="/dashboard" class="back-btn">← Dashboard</button>
        </div>
      </nav>

      <div class="container">
        <h1>🎲 Roulette</h1>
        
        <div class="game-area">
          <!-- Result Display -->
          <div class="result-display" *ngIf="lastResult && !isSpinning">
            <div class="winning-number" [style.background]="getNumberColor(lastResult.winningNumber)">
              {{ lastResult.winningNumber }}
            </div>
            <div class="result-info">
              <p>Color: {{ lastResult.color }}</p>
              <p>{{ lastResult.isEven ? 'Even' : 'Odd' }}</p>
              <p>{{ lastResult.isHigh ? 'High (19-36)' : 'Low (1-18)' }}</p>
            </div>
            <div class="total-result" [ngClass]="lastResult.totalWinAmount > 0 ? 'win' : 'lose'">
              <h3>{{ lastResult.totalWinAmount > 0 ? '🎉 YOU WIN!' : '😔 No Win' }}</h3>
              <p class="amount" *ngIf="lastResult.totalWinAmount > 0">
                \${{ lastResult.totalWinAmount.toFixed(2) }}
              </p>
            </div>
          </div>

          <!-- Spinning Animation -->
          <div class="spinning" *ngIf="isSpinning">
            <div class="wheel">🎡</div>
            <p>Spinning...</p>
          </div>

          <!-- Betting Table -->
          <div class="betting-section">
            <h3>Place Your Bets</h3>
            
            <!-- Chip Selection -->
            <div class="chip-selector">
              <div class="chip-label">Select Chip:</div>
              <button 
                *ngFor="let chip of chipValues"
                [class.selected]="selectedChip === chip"
                (click)="selectedChip = chip"
                class="chip"
              >
                \${{ chip }}
              </button>
            </div>

            <!-- Number Grid -->
            <div class="betting-grid">
              <div class="numbers-grid">
                <button 
                  *ngFor="let num of numbers"
                  (click)="placeBet('Straight', num)"
                  [class.has-bet]="hasBet('Straight', num)"
                  [style.background]="num === 0 ? '#2ecc71' : getNumberColor(num)"
                  class="number-btn"
                >
                  {{ num }}
                  <span class="bet-amount" *ngIf="getBetAmount('Straight', num) > 0">
                    \${{ getBetAmount('Straight', num) }}
                  </span>
                </button>
              </div>

              <!-- Outside Bets -->
              <div class="outside-bets">
                <button (click)="placeBet('Red')" [class.has-bet]="hasBet('Red')" class="outside-bet red-bet">
                  Red
                  <span class="bet-amount" *ngIf="getBetAmount('Red') > 0">\${{ getBetAmount('Red') }}</span>
                </button>
                <button (click)="placeBet('Black')" [class.has-bet]="hasBet('Black')" class="outside-bet black-bet">
                  Black
                  <span class="bet-amount" *ngIf="getBetAmount('Black') > 0">\${{ getBetAmount('Black') }}</span>
                </button>
                <button (click)="placeBet('Even')" [class.has-bet]="hasBet('Even')" class="outside-bet">
                  Even
                  <span class="bet-amount" *ngIf="getBetAmount('Even') > 0">\${{ getBetAmount('Even') }}</span>
                </button>
                <button (click)="placeBet('Odd')" [class.has-bet]="hasBet('Odd')" class="outside-bet">
                  Odd
                  <span class="bet-amount" *ngIf="getBetAmount('Odd') > 0">\${{ getBetAmount('Odd') }}</span>
                </button>
                <button (click)="placeBet('Low')" [class.has-bet]="hasBet('Low')" class="outside-bet">
                  1-18
                  <span class="bet-amount" *ngIf="getBetAmount('Low') > 0">\${{ getBetAmount('Low') }}</span>
                </button>
                <button (click)="placeBet('High')" [class.has-bet]="hasBet('High')" class="outside-bet">
                  19-36
                  <span class="bet-amount" *ngIf="getBetAmount('High') > 0">\${{ getBetAmount('High') }}</span>
                </button>
              </div>
            </div>

            <!-- Bet Summary -->
            <div class="bet-summary" *ngIf="bets.length > 0">
              <h4>Your Bets (Total: \${{ getTotalBetAmount() }})</h4>
              <div class="bet-list">
                <div class="bet-item" *ngFor="let bet of bets; let i = index">
                  {{ bet.betType }}{{ bet.number !== undefined ? ' ' + bet.number : '' }}: \${{ bet.amount }}
                  <button (click)="removeBet(i)" class="remove-btn">×</button>
                </div>
              </div>
            </div>

            <!-- Actions -->
            <div class="actions">
              <button 
                class="spin-btn"
                (click)="spin()"
                [disabled]="isSpinning || bets.length === 0"
              >
                {{ isSpinning ? 'SPINNING...' : 'SPIN' }}
              </button>
              <button 
                class="clear-btn"
                (click)="clearBets()"
                [disabled]="isSpinning || bets.length === 0"
              >
                Clear Bets
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
          <p><strong>1.</strong> Select a chip value</p>
          <p><strong>2.</strong> Click on numbers or bet types to place bets</p>
          <p><strong>3.</strong> Click SPIN to play</p>
          <p><strong>Payouts:</strong></p>
          <p>• Straight (single number): 35:1</p>
          <p>• Red/Black, Even/Odd, High/Low: 1:1</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .roulette-game {
      min-height: 100vh;
      background: linear-gradient(135deg, #8e2de2 0%, #4a00e0 100%);
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
      max-width: 1200px;
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
      background: white;
      border-radius: 15px;
      padding: 2rem;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
      margin-bottom: 2rem;
    }

    .result-display {
      text-align: center;
      margin-bottom: 2rem;
      padding: 1.5rem;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      border-radius: 10px;
      color: white;
    }

    .winning-number {
      width: 100px;
      height: 100px;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 3rem;
      font-weight: 700;
      color: white;
      margin: 0 auto 1rem;
      box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
    }

    .result-info p {
      margin: 0.3rem 0;
      font-size: 1.1rem;
    }

    .total-result {
      margin-top: 1rem;
      padding: 1rem;
      border-radius: 8px;
    }

    .total-result.win {
      background: rgba(46, 204, 113, 0.3);
    }

    .total-result.lose {
      background: rgba(231, 76, 60, 0.3);
    }

    .total-result h3 {
      margin-bottom: 0.5rem;
      font-size: 1.8rem;
    }

    .total-result .amount {
      font-size: 2rem;
      font-weight: 700;
    }

    .spinning {
      text-align: center;
      padding: 3rem;
      color: #667eea;
    }

    .spinning .wheel {
      font-size: 5rem;
      animation: spin 1s linear infinite;
    }

    @keyframes spin {
      from { transform: rotate(0deg); }
      to { transform: rotate(360deg); }
    }

    .spinning p {
      font-size: 1.5rem;
      font-weight: 600;
      margin-top: 1rem;
    }

    .betting-section h3 {
      color: #333;
      margin-bottom: 1.5rem;
      text-align: center;
    }

    .chip-selector {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      margin-bottom: 2rem;
      flex-wrap: wrap;
    }

    .chip-label {
      font-weight: 600;
      color: #555;
    }

    .chip {
      width: 60px;
      height: 60px;
      border-radius: 50%;
      border: 3px solid #667eea;
      background: white;
      color: #667eea;
      font-weight: 700;
      cursor: pointer;
      transition: all 0.2s;
    }

    .chip.selected {
      background: #667eea;
      color: white;
      transform: scale(1.1);
      box-shadow: 0 5px 15px rgba(102, 126, 234, 0.4);
    }

    .chip:hover {
      transform: scale(1.05);
    }

    .betting-grid {
      margin-bottom: 2rem;
    }

    .numbers-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(60px, 1fr));
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }

    .number-btn {
      aspect-ratio: 1;
      border: 2px solid white;
      color: white;
      font-weight: 700;
      font-size: 1.2rem;
      cursor: pointer;
      border-radius: 5px;
      position: relative;
      transition: transform 0.2s;
    }

    .number-btn:hover {
      transform: scale(1.05);
      box-shadow: 0 5px 15px rgba(0, 0, 0, 0.3);
    }

    .number-btn.has-bet {
      border-color: gold;
      border-width: 3px;
    }

    .outside-bets {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
      gap: 0.5rem;
    }

    .outside-bet {
      padding: 1rem;
      border: 2px solid #667eea;
      background: white;
      color: #667eea;
      font-weight: 700;
      cursor: pointer;
      border-radius: 5px;
      position: relative;
      transition: all 0.2s;
    }

    .outside-bet:hover {
      background: #667eea;
      color: white;
    }

    .outside-bet.has-bet {
      border-color: gold;
      border-width: 3px;
      background: #667eea;
      color: white;
    }

    .red-bet {
      background: #e74c3c;
      color: white;
      border-color: #c0392b;
    }

    .red-bet:hover {
      background: #c0392b;
    }

    .black-bet {
      background: #2c3e50;
      color: white;
      border-color: #1a252f;
    }

    .black-bet:hover {
      background: #1a252f;
    }

    .bet-amount {
      position: absolute;
      top: -10px;
      right: -10px;
      background: gold;
      color: #333;
      padding: 0.2rem 0.5rem;
      border-radius: 10px;
      font-size: 0.8rem;
      font-weight: 700;
    }

    .bet-summary {
      background: #f8f9fa;
      padding: 1rem;
      border-radius: 8px;
      margin-bottom: 1.5rem;
    }

    .bet-summary h4 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .bet-list {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .bet-item {
      background: white;
      padding: 0.5rem 1rem;
      border-radius: 5px;
      border: 1px solid #ddd;
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .remove-btn {
      background: #dc3545;
      color: white;
      border: none;
      border-radius: 50%;
      width: 20px;
      height: 20px;
      cursor: pointer;
      font-weight: 700;
    }

    .actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
    }

    .spin-btn {
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

    .spin-btn:hover:not(:disabled) {
      transform: translateY(-3px);
      box-shadow: 0 8px 20px rgba(56, 239, 125, 0.6);
    }

    .spin-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .clear-btn {
      padding: 1rem 2rem;
      background: #dc3545;
      color: white;
      border: none;
      border-radius: 8px;
      font-weight: 700;
      cursor: pointer;
    }

    .clear-btn:hover:not(:disabled) {
      background: #c82333;
    }

    .clear-btn:disabled {
      opacity: 0.5;
      cursor: not-allowed;
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
      .numbers-grid {
        grid-template-columns: repeat(auto-fill, minmax(45px, 1fr));
      }

      .number-btn {
        font-size: 1rem;
      }

      .chip {
        width: 50px;
        height: 50px;
      }

      .actions {
        flex-direction: column;
      }
    }
  `]
})
export class RouletteComponent implements OnInit {
  user: User | null = null;
  selectedChip: number = 5;
  chipValues: number[] = [5, 10, 25, 50, 100];
  bets: RouletteBet[] = [];
  isSpinning: boolean = false;
  lastResult: RouletteResult | null = null;
  errorMessage: string = '';
  
  numbers: number[] = Array.from({length: 37}, (_, i) => i);
  
  private redNumbers = [1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36];

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

  placeBet(betType: string, number?: number): void {
    const totalBets = this.getTotalBetAmount();
    if (totalBets + this.selectedChip > (this.user?.balance || 0)) {
      this.errorMessage = 'Insufficient balance';
      setTimeout(() => this.errorMessage = '', 3000);
      return;
    }

    this.bets.push({
      amount: this.selectedChip,
      betType: betType,
      number: number
    });
  }

  removeBet(index: number): void {
    this.bets.splice(index, 1);
  }

  clearBets(): void {
    this.bets = [];
  }

  hasBet(betType: string, number?: number): boolean {
    return this.bets.some(b => 
      b.betType === betType && 
      (number === undefined || b.number === number)
    );
  }

  getBetAmount(betType: string, number?: number): number {
    return this.bets
      .filter(b => b.betType === betType && (number === undefined || b.number === number))
      .reduce((sum, b) => sum + b.amount, 0);
  }

  getTotalBetAmount(): number {
    return this.bets.reduce((sum, bet) => sum + bet.amount, 0);
  }

  spin(): void {
    if (this.bets.length === 0) {
      this.errorMessage = 'Place at least one bet';
      return;
    }

    this.isSpinning = true;
    this.errorMessage = '';
    this.lastResult = null;

    this.gameService.spinRoulette({ bets: this.bets }).subscribe({
      next: (result) => {
        setTimeout(() => {
          this.lastResult = result;
          this.isSpinning = false;
          this.bets = [];
        }, 2000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to spin.';
        this.isSpinning = false;
      }
    });
  }

  getNumberColor(num: number): string {
    if (num === 0) return '#2ecc71';
    return this.redNumbers.includes(num) ? '#e74c3c' : '#2c3e50';
  }
}
