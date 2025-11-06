export interface Transaction {
  transactionId: number;
  userId: number;
  transactionType: 'Bet' | 'Win' | 'Deposit' | 'Withdrawal';
  amount: number;
  balanceBefore: number;
  balanceAfter: number;
  gameType?: string;
  description?: string;
  createdDate: string;
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
