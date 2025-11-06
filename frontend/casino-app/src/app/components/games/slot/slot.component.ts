import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { GameService } from '../../../services/game.service';
import { AuthService } from '../../../services/auth.service';
import { SlotResult, WinLine } from '../../../models/game.model';
import { User } from '../../../models/auth.model';

@Component({
  selector: 'app-slot',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="slot-game">
      <nav class="navbar">
        <div class="nav-brand" routerLink="/dashboard">🎰 Casino</div>
        <div class="nav-items">
          <span class="balance">Balance: \${{ (user?.balance || 0).toFixed(2) }}</span>
          <span class="username">{{ user?.username }}</span>
          <button routerLink="/dashboard" class="back-btn">← Dashboard</button>
        </div>
      </nav>

      <div class="container">
        <h1>🎰 Slot Machine</h1>
        
        <div class="game-area">
          <!-- Slot Machine Display -->
          <div class="slot-machine">
            <div class="reels" [class.spinning]="isSpinning">
              <div class="reel" *ngFor="let reel of displayReels; let i = index">
                <div class="symbol" *ngFor="let symbol of reel">
                  {{ symbol }}
                </div>
              </div>
            </div>
          </div>

          <!-- Win Lines Display -->
          <div class="win-display" *ngIf="lastResult && lastResult.winLines.length > 0">
            <h3>🎉 Winning Lines!</h3>
            <div class="win-line" *ngFor="let line of lastResult.winLines">
              Line {{ line.lineNumber }}: {{ line.symbol }} × {{ line.multiplier }} = \${{ line.winAmount.toFixed(2) }}
            </div>
            <div class="total-win">
              Total Win: \${{ lastResult.totalWinAmount.toFixed(2) }}
            </div>
          </div>

          <!-- No Win Message -->
          <div class="no-win" *ngIf="lastResult && lastResult.winLines.length === 0 && !isSpinning">
            <p>No win this time. Try again!</p>
          </div>

          <!-- Controls -->
          <div class="controls">
            <div class="bet-control">
              <label for="betAmount">Bet Amount:</label>
              <input 
                type="number" 
                id="betAmount"
                [(ngModel)]="betAmount"
                [disabled]="isSpinning"
                min="1"
                max="{{ user?.balance || 0 }}"
                step="1"
              />
            </div>
            
            <button 
              class="spin-btn"
              (click)="spin()"
              [disabled]="isSpinning || betAmount <= 0 || betAmount > (user?.balance || 0)"
            >
              {{ isSpinning ? 'SPINNING...' : 'SPIN' }}
            </button>

            <div class="quick-bets">
              <button (click)="setBet(10)" [disabled]="isSpinning">$10</button>
              <button (click)="setBet(25)" [disabled]="isSpinning">$25</button>
              <button (click)="setBet(50)" [disabled]="isSpinning">$50</button>
              <button (click)="setBet(100)" [disabled]="isSpinning">$100</button>
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
          <p>1. Set your bet amount</p>
          <p>2. Click SPIN</p>
          <p>3. Match 3 symbols on a line to win!</p>
          <p>4. Higher value symbols pay more</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .slot-game {
      min-height: 100vh;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
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

    .back-btn:hover {
      background: #5a6268;
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
      background: white;
      border-radius: 15px;
      padding: 2rem;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
      margin-bottom: 2rem;
    }

    .slot-machine {
      background: linear-gradient(145deg, #2c3e50, #34495e);
      border-radius: 10px;
      padding: 2rem;
      margin-bottom: 2rem;
      box-shadow: inset 0 5px 15px rgba(0, 0, 0, 0.3);
    }

    .reels {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: 1rem;
      background: #1a252f;
      padding: 1.5rem;
      border-radius: 8px;
    }

    .reels.spinning .reel {
      animation: spin 0.5s ease-in-out;
    }

    @keyframes spin {
      0% { transform: translateY(0); }
      50% { transform: translateY(-100px); opacity: 0.5; }
      100% { transform: translateY(0); }
    }

    .reel {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
      background: white;
      border-radius: 5px;
      padding: 1rem;
      min-height: 200px;
      justify-content: center;
      align-items: center;
    }

    .symbol {
      font-size: 3rem;
      text-align: center;
    }

    .win-display {
      background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
      color: white;
      padding: 1.5rem;
      border-radius: 10px;
      margin-bottom: 1.5rem;
      text-align: center;
    }

    .win-display h3 {
      margin-bottom: 1rem;
      font-size: 1.8rem;
    }

    .win-line {
      font-size: 1.2rem;
      margin: 0.5rem 0;
      font-weight: 600;
    }

    .total-win {
      font-size: 2rem;
      font-weight: 700;
      margin-top: 1rem;
      padding-top: 1rem;
      border-top: 2px solid white;
    }

    .no-win {
      background: #f8f9fa;
      padding: 1rem;
      border-radius: 8px;
      text-align: center;
      color: #666;
      margin-bottom: 1.5rem;
    }

    .controls {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
      align-items: center;
    }

    .bet-control {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    .bet-control label {
      font-weight: 600;
      color: #333;
    }

    .bet-control input {
      width: 150px;
      padding: 0.75rem;
      border: 2px solid #ddd;
      border-radius: 5px;
      font-size: 1.1rem;
      text-align: center;
    }

    .spin-btn {
      padding: 1.5rem 4rem;
      background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%);
      color: white;
      border: none;
      border-radius: 50px;
      font-size: 1.5rem;
      font-weight: 700;
      cursor: pointer;
      transition: transform 0.2s, box-shadow 0.2s;
      box-shadow: 0 5px 15px rgba(245, 87, 108, 0.4);
    }

    .spin-btn:hover:not(:disabled) {
      transform: translateY(-3px);
      box-shadow: 0 8px 20px rgba(245, 87, 108, 0.6);
    }

    .spin-btn:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .quick-bets {
      display: flex;
      gap: 0.5rem;
    }

    .quick-bets button {
      padding: 0.5rem 1rem;
      background: #667eea;
      color: white;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      font-weight: 600;
    }

    .quick-bets button:hover:not(:disabled) {
      background: #5568d3;
    }

    .quick-bets button:disabled {
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
      .symbol {
        font-size: 2rem;
      }

      .spin-btn {
        padding: 1rem 2rem;
        font-size: 1.2rem;
      }

      .quick-bets {
        flex-wrap: wrap;
      }
    }
  `]
})
export class SlotComponent implements OnInit {
  user: User | null = null;
  betAmount: number = 10;
  isSpinning: boolean = false;
  lastResult: SlotResult | null = null;
  errorMessage: string = '';
  
  displayReels: string[][] = [
    ['🍒', '🍋', '🍊'],
    ['🍇', '⭐', '💎'],
    ['🍉', '7️⃣', '🔔']
  ];

  private symbols = ['🍒', '🍋', '🍊', '🍇', '🍉', '⭐', '💎', '7️⃣', '🔔'];

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

  spin(): void {
    if (this.betAmount <= 0 || this.betAmount > (this.user?.balance || 0)) {
      this.errorMessage = 'Invalid bet amount';
      return;
    }

    this.isSpinning = true;
    this.errorMessage = '';
    this.lastResult = null;

    // Animate reels
    this.animateReels();

    this.gameService.spinSlot({ betAmount: this.betAmount }).subscribe({
      next: (result) => {
        setTimeout(() => {
          this.displayReels = result.reels;
          this.lastResult = result;
          this.isSpinning = false;
        }, 1000);
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to spin. Please try again.';
        this.isSpinning = false;
      }
    });
  }

  private animateReels(): void {
    const interval = setInterval(() => {
      if (!this.isSpinning) {
        clearInterval(interval);
        return;
      }
      
      this.displayReels = this.displayReels.map(() => [
        this.getRandomSymbol(),
        this.getRandomSymbol(),
        this.getRandomSymbol()
      ]);
    }, 100);
  }

  private getRandomSymbol(): string {
    return this.symbols[Math.floor(Math.random() * this.symbols.length)];
  }
}
