import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

/**
 * Service for reward-related API operations.
 * Manages customer balance, transaction history, and reward calculations.
 */
@Injectable({
  providedIn: 'root'
})
export class RewardService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/rewards`;
  
  // Reactive signals for customer data
  public currentBalance = signal<number>(0);
  public lifetimePoints = signal<number>(0);
  public currentTier = signal<string>('Bronze');
  public progressToNextTier = signal<number>(0);

  constructor(private http: HttpClient) {}

  /**
   * Gets customer's current points balance and tier information.
   */
  public getBalance(customerId: string): Observable<CustomerBalanceResponse> {
    return this.http.get<CustomerBalanceResponse>(`${this.apiUrl}/balance/${customerId}`)
      .pipe(
        tap(response => {
          this.currentBalance.set(response.pointsBalance);
          this.lifetimePoints.set(response.lifetimePoints);
          this.currentTier.set(response.tier);
          this.progressToNextTier.set(response.progressToNextTier);
        })
      );
  }

  /**
   * Gets customer's reward transaction history.
   */
  public getHistory(
    customerId: string,
    pageNumber: number = 1,
    pageSize: number = 20,
    rewardType?: string,
    fromDate?: Date,
    toDate?: Date
  ): Observable<RewardHistoryResponse> {
    let params: any = {
      pageNumber: pageNumber.toString(),
      pageSize: pageSize.toString()
    };

    if (rewardType) params.rewardType = rewardType;
    if (fromDate) params.fromDate = fromDate.toISOString();
    if (toDate) params.toDate = toDate.toISOString();

    return this.http.get<RewardHistoryResponse>(`${this.apiUrl}/history/${customerId}`, { params });
  }

  /**
   * Calculates and awards reward points for a transaction.
   */
  public calculateReward(request: CalculateRewardRequest): Observable<CalculateRewardResponse> {
    return this.http.post<CalculateRewardResponse>(`${this.apiUrl}/calculate`, request);
  }

  /**
   * Redeems loyalty points.
   */
  public redeemPoints(request: RedeemPointsRequest): Observable<RedeemPointsResponse> {
    return this.http.post<RedeemPointsResponse>(`${this.apiUrl}/redeem`, request)
      .pipe(
        tap(response => {
          this.currentBalance.set(response.newBalance);
        })
      );
  }

  /**
   * Updates balance from SignalR notification.
   */
  public updateBalanceFromNotification(newBalance: number): void {
    this.currentBalance.set(newBalance);
  }

  /**
   * Updates tier from SignalR notification.
   */
  public updateTierFromNotification(newTier: string): void {
    this.currentTier.set(newTier);
  }
}

// ============================================
// Request/Response Types
// ============================================

export interface CustomerBalanceResponse {
  customerId: string;
  pointsBalance: number;
  lifetimePoints: number;
  tier: string;
  progressToNextTier: number;
  nextTierThreshold: number;
  nextTierName: string;
}

export interface RewardHistoryResponse {
  rewards: RewardHistoryItem[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface RewardHistoryItem {
  id: string;
  pointsAwarded: number;
  transactionAmount: number;
  currency: string;
  rewardType: string;
  source: string;
  description: string;
  processedAt: string;
}

export interface CalculateRewardRequest {
  customerId: string;
  transactionAmount: number;
  currency?: string;
  transactionId?: string;
  merchantId?: string;
  merchantCategory?: string;
  productCategory?: string;
  transactionDate?: string;
}

export interface CalculateRewardResponse {
  success: boolean;
  pointsAwarded: number;
  newBalance: number;
  tier: string;
  message: string;
}

export interface RedeemPointsRequest {
  customerId: string;
  pointsToRedeem: number;
  redemptionType: string;
  redemptionDetails?: string;
}

export interface RedeemPointsResponse {
  success: boolean;
  pointsRedeemed: number;
  newBalance: number;
  message: string;
}
