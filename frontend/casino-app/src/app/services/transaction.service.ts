import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction, PaginatedResult } from '../models/transaction.model';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class TransactionService {
  private readonly API_URL = environment.apiUrl;

  constructor(private http: HttpClient) {}

  getTransactions(pageNumber: number = 1, pageSize: number = 20): Observable<PaginatedResult<Transaction>> {
    return this.http.get<PaginatedResult<Transaction>>(
      `${this.API_URL}/transactions?pageNumber=${pageNumber}&pageSize=${pageSize}`
    );
  }

  getTransaction(id: number): Observable<Transaction> {
    return this.http.get<Transaction>(`${this.API_URL}/transactions/${id}`);
  }
}
