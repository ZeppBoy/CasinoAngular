import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { 
  SlotSpinRequest, 
  SlotResult, 
  BlackjackStartRequest, 
  BlackjackState,
  RouletteSpinRequest,
  RouletteResult,
  PokerStartRequest,
  PokerState,
  PokerDrawRequest
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

  // Poker
  startPoker(request: PokerStartRequest): Observable<PokerState> {
    return this.http.post<PokerState>(`${this.API_URL}/games/poker/start`, request)
      .pipe(tap(state => this.updateBalance(state.balanceAfter)));
  }

  drawPoker(gameId: string, request: PokerDrawRequest): Observable<PokerState> {
    return this.http.post<PokerState>(`${this.API_URL}/games/poker/${gameId}/draw`, request)
      .pipe(tap(state => this.updateBalance(state.balanceAfter)));
  }

  getPokerGame(gameId: string): Observable<PokerState> {
    return this.http.get<PokerState>(`${this.API_URL}/games/poker/${gameId}`);
  }

  private updateBalance(newBalance: number): void {
    const user = this.authService.currentUserValue;
    if (user) {
      user.balance = newBalance;
      this.authService.updateCurrentUser(user);
    }
  }
}
