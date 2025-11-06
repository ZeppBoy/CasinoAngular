import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { 
  SlotSpinRequest, 
  SlotResult, 
  BlackjackStartRequest, 
  BlackjackState,
  RouletteSpinRequest,
  RouletteResult
} from '../models/game.model';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class GameService {
  private readonly API_URL = environment.apiUrl;

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  // Slot Machine
  spinSlot(request: SlotSpinRequest): Observable<SlotResult> {
    return this.http.post<SlotResult>(`${this.API_URL}/games/slot/spin`, request)
      .pipe(tap(result => this.updateBalance(result.balanceAfter)));
  }

  // Blackjack
  startBlackjack(request: BlackjackStartRequest): Observable<BlackjackState> {
    return this.http.post<BlackjackState>(`${this.API_URL}/games/blackjack/start`, request)
      .pipe(tap(state => this.updateBalance(state.balanceAfter)));
  }

  hitBlackjack(gameId: string): Observable<BlackjackState> {
    return this.http.post<BlackjackState>(`${this.API_URL}/games/blackjack/${gameId}/hit`, {})
      .pipe(tap(state => this.updateBalance(state.balanceAfter)));
  }

  standBlackjack(gameId: string): Observable<BlackjackState> {
    return this.http.post<BlackjackState>(`${this.API_URL}/games/blackjack/${gameId}/stand`, {})
      .pipe(tap(state => this.updateBalance(state.balanceAfter)));
  }

  doubleBlackjack(gameId: string): Observable<BlackjackState> {
    return this.http.post<BlackjackState>(`${this.API_URL}/games/blackjack/${gameId}/double`, {})
      .pipe(tap(state => this.updateBalance(state.balanceAfter)));
  }

  // Roulette
  spinRoulette(request: RouletteSpinRequest): Observable<RouletteResult> {
    return this.http.post<RouletteResult>(`${this.API_URL}/games/roulette/spin`, request)
      .pipe(tap(result => this.updateBalance(result.balanceAfter)));
  }

  private updateBalance(newBalance: number): void {
    const user = this.authService.currentUserValue;
    if (user) {
      user.balance = newBalance;
      this.authService.updateCurrentUser(user);
    }
  }
}
