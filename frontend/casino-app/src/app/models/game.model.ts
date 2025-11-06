export interface SlotSpinRequest {
  betAmount: number;
}

export interface SlotResult {
  reels: string[][];
  winLines: WinLine[];
  totalWinAmount: number;
  balanceAfter: number;
}

export interface WinLine {
  lineNumber: number;
  symbol: string;
  multiplier: number;
  winAmount: number;
}

// Blackjack
export interface Card {
  suit: string;
  rank: string;
  value: number;
}

export interface BlackjackStartRequest {
  betAmount: number;
}

export interface BlackjackState {
  gameId: string;
  playerHand: Card[];
  dealerHand: Card[];
  playerHandValue: number;
  dealerHandValue: number;
  dealerShowingHoleCard: boolean;
  status: string;
  winAmount?: number;
  balanceAfter: number;
  canHit: boolean;
  canStand: boolean;
  canDouble: boolean;
}

// Roulette
export interface RouletteBet {
  amount: number;
  betType: string;
  number?: number;
  range?: string;
}

export interface RouletteSpinRequest {
  bets: RouletteBet[];
}

export interface RouletteResult {
  winningNumber: number;
  color: string;
  isEven: boolean;
  isHigh: boolean;
  betResults: RouletteBetResult[];
  totalWinAmount: number;
  balanceAfter: number;
}

export interface RouletteBetResult {
  betType: string;
  betAmount: number;
  isWin: boolean;
  winAmount: number;
  payout: number;
}
