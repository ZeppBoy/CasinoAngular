import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { UserService } from '../../services/user.service';
import { AuthService } from '../../services/auth.service';
import { User } from '../../models/auth.model';
import { ToastService } from '../../shared/services/toast.service';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner.component';

@Component({
  selector: 'app-account',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, LoadingSpinnerComponent],
  template: `
    <app-loading-spinner [show]="isLoading" message="Processing..."></app-loading-spinner>
    <div class="account-page">
      <nav class="navbar">
        <div class="nav-brand" routerLink="/dashboard">🎰 Casino</div>
        <div class="nav-items">
          <span class="balance">Balance: \${{ (user?.balance || 0).toFixed(2) }}</span>
          <span class="username">{{ user?.username }}</span>
          <button routerLink="/dashboard" class="back-btn">← Dashboard</button>
        </div>
      </nav>

      <div class="container">
        <h1>My Account</h1>

        <div class="account-grid">
          <div class="card profile-card">
            <h2>Profile Information</h2>
            <div class="info-row">
              <label>Username:</label>
              <span>{{ user?.username }}</span>
            </div>
            <div class="info-row">
              <label>Email:</label>
              <span>{{ user?.email }}</span>
            </div>
            <div class="info-row">
              <label>Member Since:</label>
              <span>{{ formatDate(user?.createdDate) }}</span>
            </div>
            <div class="info-row">
              <label>Current Balance:</label>
              <span class="balance-amount">\${{ (user?.balance || 0).toFixed(2) }}</span>
            </div>
          </div>

          <div class="card actions-card">
            <h2>Quick Actions</h2>
            
            <div class="action-section">
              <h3>💰 Deposit Funds</h3>
              <div class="form-group">
                <input 
                  type="number" 
                  [(ngModel)]="depositAmount" 
                  placeholder="Amount"
                  min="0.01"
                  max="10000"
                  step="0.01">
                <button (click)="deposit()" [disabled]="isLoading">
                  {{ isLoading ? 'Processing...' : 'Deposit' }}
                </button>
              </div>
            </div>

            <div class="action-section">
              <h3>💸 Withdraw Funds</h3>
              <div class="form-group">
                <input 
                  type="number" 
                  [(ngModel)]="withdrawAmount" 
                  placeholder="Amount"
                  min="0.01"
                  [max]="user?.balance || 0"
                  step="0.01">
                <button (click)="withdraw()" [disabled]="isLoading" class="withdraw-btn">
                  {{ isLoading ? 'Processing...' : 'Withdraw' }}
                </button>
              </div>
            </div>


          </div>

          <div class="card quick-links-card">
            <h2>Quick Links</h2>
            <button routerLink="/transactions" class="link-btn">
              📊 Transaction History
            </button>
            <button routerLink="/games/slot" class="link-btn">
              🎰 Play Slot Machine
            </button>
            <button routerLink="/games/blackjack" class="link-btn">
              🃏 Play Blackjack
            </button>
            <button routerLink="/games/roulette" class="link-btn">
              🎲 Play Roulette
            </button>
            <button routerLink="/games/poker" class="link-btn">
              🂡 Play Video Poker
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .account-page { min-height: 100vh; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding-bottom: 2rem; }
    .navbar { background: rgba(255, 255, 255, 0.1); backdrop-filter: blur(10px); padding: 1rem 2rem; display: flex; justify-content: space-between; align-items: center; }
    .nav-brand { font-size: 1.5rem; font-weight: bold; color: white; cursor: pointer; }
    .nav-items { display: flex; gap: 2rem; align-items: center; color: white; }
    .balance { font-weight: 600; font-size: 1.1rem; }
    .back-btn { background: rgba(255, 255, 255, 0.2); color: white; border: none; padding: 0.5rem 1rem; border-radius: 8px; cursor: pointer; }
    .container { max-width: 1200px; margin: 0 auto; padding: 2rem; }
    h1 { text-align: center; color: white; margin-bottom: 2rem; font-size: 2.5rem; text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3); }
    .account-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(350px, 1fr)); gap: 2rem; }
    .card { background: white; border-radius: 16px; padding: 2rem; box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2); }
    .card h2 { color: #333; margin-bottom: 1.5rem; font-size: 1.5rem; border-bottom: 2px solid #667eea; padding-bottom: 0.5rem; }
    .info-row { display: flex; justify-content: space-between; padding: 1rem 0; border-bottom: 1px solid #eee; }
    .info-row label { font-weight: 600; color: #666; }
    .balance-amount { color: #28a745; font-weight: bold; font-size: 1.2rem; }
    .action-section { margin-bottom: 2rem; }
    .action-section h3 { color: #667eea; margin-bottom: 1rem; }
    .form-group { display: flex; gap: 1rem; }
    .form-group input { flex: 1; padding: 0.75rem; border: 2px solid #ddd; border-radius: 8px; }
    .form-group button { padding: 0.75rem 1.5rem; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer; }
    .form-group button:disabled { background: #ccc; }
    .form-group button.withdraw-btn { background: #dc3545; }
    .success-message { margin-top: 1rem; padding: 1rem; background: #d4edda; color: #155724; border-radius: 8px; }
    .error-message { margin-top: 1rem; padding: 1rem; background: #f8d7da; color: #721c24; border-radius: 8px; }
    .link-btn { display: block; width: 100%; padding: 1rem; margin-bottom: 1rem; background: #f5f5f5; border: 2px solid #ddd; border-radius: 8px; cursor: pointer; text-align: left; transition: all 0.3s; }
    .link-btn:hover { background: #667eea; color: white; transform: translateX(5px); }
  `]
})
export class AccountComponent implements OnInit {
  user: User | null = null;
  depositAmount: number = 10;
  withdrawAmount: number = 10;
  isLoading: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.user = user;
      if (!user) this.router.navigate(['/login']);
    });
  }

  deposit(): void {
    if (this.depositAmount < 0.01 || this.depositAmount > 10000) {
      this.toastService.error('Deposit amount must be between $0.01 and $10,000', 'Invalid Amount');
      return;
    }

    this.isLoading = true;

    this.userService.deposit(this.depositAmount).subscribe({
      next: (response) => {
        this.toastService.success(`Successfully deposited $${this.depositAmount.toFixed(2)}`, 'Deposit Complete');
        this.depositAmount = 10;
        this.isLoading = false;
      },
      error: (error) => {
        this.toastService.error(error.error?.message || 'Deposit failed', 'Deposit Error');
        this.isLoading = false;
      }
    });
  }

  withdraw(): void {
    if (this.withdrawAmount < 0.01 || this.withdrawAmount > (this.user?.balance || 0)) {
      this.toastService.error('Invalid withdrawal amount', 'Withdrawal Error');
      return;
    }

    this.isLoading = true;

    this.userService.withdraw(this.withdrawAmount).subscribe({
      next: (response) => {
        this.toastService.success(`Successfully withdrew $${this.withdrawAmount.toFixed(2)}`, 'Withdrawal Complete');
        this.withdrawAmount = 10;
        this.isLoading = false;
      },
      error: (error) => {
        this.toastService.error(error.error?.message || 'Withdrawal failed', 'Withdrawal Error');
        this.isLoading = false;
      }
    });
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return 'N/A';
    return new Date(date).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });
  }
}
