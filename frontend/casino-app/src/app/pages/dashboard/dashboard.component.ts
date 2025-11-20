import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/auth.model';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="dashboard">
      <nav class="navbar">
        <div class="nav-brand">🎰 Casino</div>
        <div class="nav-items">
          <span class="balance">Balance: \${{ (user?.balance || 0).toFixed(2) }}</span>
          <span class="username">{{ user?.username }}</span>
          <button (click)="logout()" class="logout-btn">Logout</button>
        </div>
      </nav>

      <div class="container">
        <h1>Welcome to the Casino!</h1>
        
        <div class="games-grid">
          <div class="game-card" routerLink="/games/slot">
            <div class="game-icon">🎰</div>
            <h3>Slot Machine</h3>
            <p>Spin the reels and win big!</p>
            <button>Play Now</button>
          </div>

          <div class="game-card" routerLink="/games/blackjack">
            <div class="game-icon">🃏</div>
            <h3>Blackjack</h3>
            <p>Beat the dealer to 21!</p>
            <button>Play Now</button>
          </div>

          <div class="game-card" routerLink="/games/roulette">
            <div class="game-icon">🎲</div>
            <h3>Roulette</h3>
            <p>Place your bets and spin!</p>
            <button>Play Now</button>
          </div>

          <div class="game-card" routerLink="/games/poker">
            <div class="game-icon">🂡</div>
            <h3>Video Poker</h3>
            <p>Jacks or Better - 5 Card Draw!</p>
            <button>Play Now</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard {
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

    .logout-btn {
      padding: 0.5rem 1rem;
      background: #dc3545;
      color: white;
      border: none;
      border-radius: 5px;
      cursor: pointer;
      font-weight: 600;
    }

    .logout-btn:hover {
      background: #c82333;
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

    .games-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
      gap: 2rem;
      margin-bottom: 3rem;
    }

    .game-card {
      background: white;
      border-radius: 10px;
      padding: 2rem;
      text-align: center;
      cursor: pointer;
      transition: transform 0.3s, box-shadow 0.3s;
    }

    .game-card:hover {
      transform: translateY(-10px);
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
    }

    .game-icon {
      font-size: 4rem;
      margin-bottom: 1rem;
    }

    .game-card h3 {
      color: #333;
      margin-bottom: 0.5rem;
    }

    .game-card p {
      color: #666;
      margin-bottom: 1rem;
    }

    .game-card button {
      padding: 0.75rem 1.5rem;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border: none;
      border-radius: 5px;
      font-weight: 600;
      cursor: pointer;
    }
  `]
})
export class DashboardComponent implements OnInit {
  user: User | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.user = user;
    });
  }

  logout(): void {
    this.authService.logout().subscribe(() => {
      this.router.navigate(['/login']);
    });
  }
}
