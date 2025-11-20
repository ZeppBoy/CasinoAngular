import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { TransactionService } from '../../services/transaction.service';
import { AuthService } from '../../services/auth.service';
import { Transaction, PaginatedResult } from '../../models/transaction.model';
import { User } from '../../models/auth.model';

@Component({
  selector: 'app-transactions',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="transactions-page">
      <nav class="navbar">
        <div class="nav-brand" routerLink="/dashboard">🎰 Casino</div>
        <div class="nav-items">
          <span class="balance">Balance: \${{ (user?.balance || 0).toFixed(2) }}</span>
          <span class="username">{{ user?.username }}</span>
          <button routerLink="/dashboard" class="back-btn">← Dashboard</button>
        </div>
      </nav>

      <div class="container">
        <h1>Transaction History</h1>

        <div class="content-area">
          <div class="loading" *ngIf="isLoading">
            <div class="spinner"></div>
            <p>Loading transactions...</p>
          </div>

          <div class="error" *ngIf="errorMessage && !isLoading">
            <p>{{ errorMessage }}</p>
            <button (click)="loadTransactions()">Retry</button>
          </div>

          <div class="empty-state" *ngIf="!isLoading && !errorMessage && transactions.length === 0">
            <p>No transactions yet</p>
            <button routerLink="/dashboard" class="primary-btn">Start Playing</button>
          </div>

          <div class="transactions-table" *ngIf="!isLoading && transactions.length > 0">
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Type</th>
                  <th>Game</th>
                  <th>Amount</th>
                  <th>Balance After</th>
                  <th>Description</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let transaction of transactions" 
                    [class.win]="transaction.transactionType === 'Win'"
                    [class.bet]="transaction.transactionType === 'Bet'">
                  <td>{{ formatDate(transaction.createdDate) }}</td>
                  <td>
                    <span class="type-badge" [attr.data-type]="transaction.transactionType">
                      {{ transaction.transactionType }}
                    </span>
                  </td>
                  <td>{{ transaction.gameType || '-' }}</td>
                  <td [class.positive]="isPositive(transaction)" 
                      [class.negative]="!isPositive(transaction)">
                    {{ formatAmount(transaction) }}
                  </td>
                  <td>\${{ transaction.balanceAfter.toFixed(2) }}</td>
                  <td class="description">{{ transaction.description || '-' }}</td>
                </tr>
              </tbody>
            </table>

            <div class="pagination" *ngIf="totalPages > 1">
              <button (click)="previousPage()" [disabled]="currentPage === 1" class="page-btn">
                ← Previous
              </button>
              <span class="page-info">
                Page {{ currentPage }} of {{ totalPages }}
              </span>
              <button (click)="nextPage()" [disabled]="currentPage === totalPages" class="page-btn">
                Next →
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .transactions-page { min-height: 100vh; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); padding-bottom: 2rem; }
    .navbar { background: rgba(255, 255, 255, 0.1); backdrop-filter: blur(10px); padding: 1rem 2rem; display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid rgba(255, 255, 255, 0.2); }
    .nav-brand { font-size: 1.5rem; font-weight: bold; color: white; cursor: pointer; }
    .nav-items { display: flex; gap: 2rem; align-items: center; color: white; }
    .balance { font-weight: 600; font-size: 1.1rem; }
    .back-btn { background: rgba(255, 255, 255, 0.2); color: white; border: none; padding: 0.5rem 1rem; border-radius: 8px; cursor: pointer; }
    .container { max-width: 1400px; margin: 0 auto; padding: 2rem; }
    h1 { text-align: center; color: white; margin-bottom: 2rem; font-size: 2.5rem; text-shadow: 2px 2px 4px rgba(0, 0, 0, 0.3); }
    .content-area { background: white; border-radius: 16px; padding: 2rem; box-shadow: 0 8px 32px rgba(0, 0, 0, 0.2); min-height: 400px; }
    .loading, .empty-state { text-align: center; padding: 3rem; }
    .spinner { border: 4px solid #f3f3f3; border-top: 4px solid #667eea; border-radius: 50%; width: 50px; height: 50px; animation: spin 1s linear infinite; margin: 0 auto 1rem; }
    @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
    .error { text-align: center; padding: 2rem; color: #d32f2f; }
    .error button { margin-top: 1rem; padding: 0.75rem 2rem; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer; }
    .primary-btn { padding: 1rem 2rem; font-size: 1.1rem; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; border: none; border-radius: 12px; cursor: pointer; }
    .transactions-table { overflow-x: auto; }
    table { width: 100%; border-collapse: collapse; }
    thead { background: #f5f5f5; }
    th { padding: 1rem; text-align: left; font-weight: 600; color: #333; border-bottom: 2px solid #ddd; }
    td { padding: 0.75rem 1rem; border-bottom: 1px solid #eee; }
    tbody tr:hover { background: #f9f9f9; }
    tbody tr.win { background: #f0fff4; }
    .type-badge { display: inline-block; padding: 0.25rem 0.75rem; border-radius: 12px; font-size: 0.85rem; font-weight: 600; }
    .type-badge[data-type="Win"] { background: #d4edda; color: #155724; }
    .type-badge[data-type="Bet"] { background: #fff3cd; color: #856404; }
    .type-badge[data-type="Deposit"] { background: #d1ecf1; color: #0c5460; }
    .type-badge[data-type="Withdrawal"] { background: #f8d7da; color: #721c24; }
    .positive { color: #28a745; font-weight: 600; }
    .negative { color: #dc3545; font-weight: 600; }
    .description { max-width: 300px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .pagination { display: flex; justify-content: space-between; align-items: center; margin-top: 2rem; padding-top: 1rem; border-top: 2px solid #eee; }
    .page-btn { padding: 0.75rem 1.5rem; background: #667eea; color: white; border: none; border-radius: 8px; cursor: pointer; }
    .page-btn:disabled { background: #ccc; cursor: not-allowed; }
  `]
})
export class TransactionsComponent implements OnInit {
  user: User | null = null;
  transactions: Transaction[] = [];
  currentPage: number = 1;
  pageSize: number = 20;
  totalCount: number = 0;
  totalPages: number = 0;
  isLoading: boolean = false;
  errorMessage: string = '';

  constructor(
    private transactionService: TransactionService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.authService.currentUser.subscribe(user => {
      this.user = user;
      if (!user) {
        this.router.navigate(['/login']);
      } else {
        this.loadTransactions();
      }
    });
  }

  loadTransactions(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.transactionService.getTransactions(this.currentPage, this.pageSize).subscribe({
      next: (result: PaginatedResult<Transaction>) => {
        this.transactions = result.items;
        this.currentPage = result.pageNumber;
        this.totalPages = result.totalPages;
        this.totalCount = result.totalCount;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Failed to load transactions';
        this.isLoading = false;
      }
    });
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
      this.loadTransactions();
    }
  }

  previousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.loadTransactions();
    }
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    return d.toLocaleString('en-US', {
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  formatAmount(transaction: Transaction): string {
    const amount = transaction.amount;
    const prefix = this.isPositive(transaction) ? '+$' : '-$';
    return prefix + Math.abs(amount).toFixed(2);
  }

  isPositive(transaction: Transaction): boolean {
    return transaction.transactionType === 'Win' || transaction.transactionType === 'Deposit';
  }
}
