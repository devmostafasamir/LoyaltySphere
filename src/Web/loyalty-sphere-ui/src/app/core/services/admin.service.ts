import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, finalize } from 'rxjs';
import { environment } from '../../../environments/environment';

// ============================================
// INTERFACES
// ============================================

export interface Campaign {
  id: string;
  campaignName: string;
  description: string;
  campaignType: 'Bonus' | 'Multiplier' | 'Cashback';
  bonusPoints: number;
  pointsMultiplier?: number;
  cashbackPercentage?: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  targetCustomerSegment?: string;
  targetMerchantCategory?: string;
  minimumTransactionAmount?: number;
  maxParticipations?: number;
  currentParticipations: number;
  terms?: string;
  createdAt: string;
}

export interface CreateCampaignRequest {
  campaignName: string;
  description: string;
  campaignType: 'Bonus' | 'Multiplier' | 'Cashback';
  startDate: string;
  endDate: string;
  bonusPoints?: number;
  pointsMultiplier?: number;
  cashbackPercentage?: number;
  targetCustomerSegment?: string;
  targetMerchantCategory?: string;
  minimumTransactionAmount?: number;
  maxParticipations?: number;
  terms?: string;
}

export interface RewardRule {
  id: string;
  ruleName: string;
  description: string;
  pointsPerUnit: number;
  minimumTransactionAmount?: number;
  maximumTransactionAmount?: number;
  merchantCategory?: string;
  merchantId?: string;
  productCategory?: string;
  priority: number;
  isActive: boolean;
  validFrom?: string;
  validUntil?: string;
  ruleType: string;
  createdAt: string;
}

export interface CreateRewardRuleRequest {
  ruleName: string;
  description: string;
  pointsPerUnit: number;
  ruleType?: string;
  priority: number;
  minimumTransactionAmount?: number;
  maximumTransactionAmount?: number;
  merchantCategory?: string;
  merchantId?: string;
  productCategory?: string;
  validFrom?: string;
  validUntil?: string;
}

export interface UpdateRewardRuleRequest {
  ruleName: string;
  description: string;
  pointsPerUnit: number;
  priority: number;
  minimumTransactionAmount?: number;
  maximumTransactionAmount?: number;
}

export interface DashboardAnalytics {
  totalCustomers: number;
  activeCustomers: number;
  totalPointsAwarded: number;
  totalPointsRedeemed: number;
  activeCampaigns: number;
  tierDistribution: TierDistribution[];
  recentTransactions: DailyTransaction[];
  fromDate: string;
  toDate: string;
}

export interface TierDistribution {
  tier: string;
  count: number;
  totalPoints: number;
}

export interface DailyTransaction {
  date: string;
  count: number;
  totalPoints: number;
}

/**
 * Admin service for managing campaigns, reward rules, and analytics.
 * Provides reactive state management with Angular signals.
 */
@Injectable({
  providedIn: 'root'
})
export class AdminService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api/v1/admin`;

  // Reactive state with signals
  campaigns = signal<Campaign[]>([]);
  rewardRules = signal<RewardRule[]>([]);
  analytics = signal<DashboardAnalytics | null>(null);
  loading = signal(false);

  // ============================================
  // CAMPAIGN MANAGEMENT
  // ============================================

  /**
   * Gets all campaigns for the current tenant.
   */
  getCampaigns(isActive?: boolean): Observable<Campaign[]> {
    this.loading.set(true);
    const options = isActive !== undefined 
      ? { params: { isActive: isActive.toString() } }
      : {};

    return this.http.get<Campaign[]>(`${this.baseUrl}/campaigns`, options).pipe(
      tap(campaigns => this.campaigns.set(campaigns)),
      finalize(() => this.loading.set(false))
    );
  }

  /**
   * Gets a specific campaign by ID.
   */
  getCampaign(id: string): Observable<Campaign> {
    return this.http.get<Campaign>(`${this.baseUrl}/campaigns/${id}`);
  }

  /**
   * Creates a new campaign.
   */
  createCampaign(request: CreateCampaignRequest): Observable<Campaign> {
    this.loading.set(true);
    return this.http.post<Campaign>(`${this.baseUrl}/campaigns`, request).pipe(
      tap(campaign => this.campaigns.update(campaigns => [...campaigns, campaign])),
      finalize(() => this.loading.set(false))
    );
  }

  /**
   * Activates a campaign.
   */
  activateCampaign(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/campaigns/${id}/activate`, {}).pipe(
      tap(() => {
        this.campaigns.update(campaigns =>
          campaigns.map(c => c.id === id ? { ...c, isActive: true } : c)
        );
      })
    );
  }

  /**
   * Deactivates a campaign.
   */
  deactivateCampaign(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/campaigns/${id}/deactivate`, {}).pipe(
      tap(() => {
        this.campaigns.update(campaigns =>
          campaigns.map(c => c.id === id ? { ...c, isActive: false } : c)
        );
      })
    );
  }

  // ============================================
  // REWARD RULES MANAGEMENT
  // ============================================

  /**
   * Gets all reward rules for the current tenant.
   */
  getRewardRules(isActive?: boolean): Observable<RewardRule[]> {
    this.loading.set(true);
    const options = isActive !== undefined 
      ? { params: { isActive: isActive.toString() } }
      : {};

    return this.http.get<RewardRule[]>(`${this.baseUrl}/reward-rules`, options).pipe(
      tap(rules => this.rewardRules.set(rules)),
      finalize(() => this.loading.set(false))
    );
  }

  /**
   * Creates a new reward rule.
   */
  createRewardRule(request: CreateRewardRuleRequest): Observable<RewardRule> {
    this.loading.set(true);
    return this.http.post<RewardRule>(`${this.baseUrl}/reward-rules`, request).pipe(
      tap(rule => this.rewardRules.update(rules => [...rules, rule])),
      finalize(() => this.loading.set(false))
    );
  }

  /**
   * Updates an existing reward rule.
   */
  updateRewardRule(id: string, request: UpdateRewardRuleRequest): Observable<RewardRule> {
    return this.http.put<RewardRule>(`${this.baseUrl}/reward-rules/${id}`, request).pipe(
      tap(updatedRule => {
        this.rewardRules.update(rules =>
          rules.map(r => r.id === id ? updatedRule : r)
        );
      })
    );
  }

  /**
   * Activates a reward rule.
   */
  activateRewardRule(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/reward-rules/${id}/activate`, {}).pipe(
      tap(() => {
        this.rewardRules.update(rules =>
          rules.map(r => r.id === id ? { ...r, isActive: true } : r)
        );
      })
    );
  }

  /**
   * Deactivates a reward rule.
   */
  deactivateRewardRule(id: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/reward-rules/${id}/deactivate`, {}).pipe(
      tap(() => {
        this.rewardRules.update(rules =>
          rules.map(r => r.id === id ? { ...r, isActive: false } : r)
        );
      })
    );
  }

  // ============================================
  // ANALYTICS & DASHBOARD
  // ============================================

  /**
   * Gets dashboard analytics for the current tenant.
   */
  getDashboardAnalytics(fromDate?: string, toDate?: string): Observable<DashboardAnalytics> {
    this.loading.set(true);
    const params: any = {};
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;

    return this.http.get<DashboardAnalytics>(`${this.baseUrl}/analytics/dashboard`, { params }).pipe(
      tap(analytics => {
        this.analytics.set(analytics);
        this.loading.set(false);
      })
    );
  }
}
