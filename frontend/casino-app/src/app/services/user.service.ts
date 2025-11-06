import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { User } from '../models/auth.model';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly API_URL = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  getProfile(): Observable<User> {
    return this.http.get<User>(`${this.API_URL}/users/profile`)
      .pipe(
        tap(user => this.authService.updateCurrentUser(user))
      );
  }

  updateProfile(data: { username?: string; email?: string }): Observable<User> {
    return this.http.put<User>(`${this.API_URL}/users/profile`, data)
      .pipe(
        tap(user => this.authService.updateCurrentUser(user))
      );
  }

  getBalance(): Observable<{ balance: number }> {
    return this.http.get<{ balance: number }>(`${this.API_URL}/users/balance`);
  }

  deposit(amount: number): Observable<{ balance: number }> {
    return this.http.post<{ balance: number }>(`${this.API_URL}/users/deposit`, { amount })
      .pipe(
        tap(response => {
          const user = this.authService.currentUserValue;
          if (user) {
            user.balance = response.balance;
            this.authService.updateCurrentUser(user);
          }
        })
      );
  }

  withdraw(amount: number): Observable<{ balance: number }> {
    return this.http.post<{ balance: number }>(`${this.API_URL}/users/withdraw`, { amount })
      .pipe(
        tap(response => {
          const user = this.authService.currentUserValue;
          if (user) {
            user.balance = response.balance;
            this.authService.updateCurrentUser(user);
          }
        })
      );
  }
}
