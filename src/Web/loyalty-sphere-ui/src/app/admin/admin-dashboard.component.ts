import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminService, DashboardAnalytics } from '../core/services/admin.service';

/**
 * Admin dashboard component showing key metrics and analytics.
 * Displays customer stats, points activity, tier distribution, and recent transactions.
 */
@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-slate-950 p-6">
      <!-- Header -->
      <div class="mb-8">
        <h1 class="text-3xl font-bold text-white mb-2">Admin Dashboard</h1>
        <p class="text-slate-400">Manage your loyalty program and view analytics</p>
      </div>

      <!-- Loading State -->
      @if (adminService.loading()) {
        <div class="flex items-center justify-center h-64">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-700"></div>
        </div>
      }

      <!-- Analytics Content -->
      @if (analytics() && !adminService.loading()) {
        <!-- Key Metrics Grid -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 mb-8">
          <!-- Total Customers -->
          <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6 hover:border-rose-700/50 transition-all">
            <div class="flex items-center justify-between mb-4">
              <div class="p-3 bg-rose-700/10 rounded-lg">
                <svg class="w-6 h-6 text-rose-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
              </div>
            </div>
            <h3 class="text-slate-400 text-sm font-medium mb-1">Total Customers</h3>
            <p class="text-3xl font-bold text-white">{{ analytics()!.totalCustomers | number }}</p>
            <p class="text-sm text-emerald-400 mt-2">
              {{ analytics()!.activeCustomers | number }} active
            </p>
          </div>

          <!-- Points Awarded -->
          <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6 hover:border-amber-500/50 transition-all">
            <div class="flex items-center justify-between mb-4">
              <div class="p-3 bg-amber-500/10 rounded-lg">
                <svg class="w-6 h-6 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8c-1.657 0-3 .895-3 2s1.343 2 3 2 3 .895 3 2-1.343 2-3 2m0-8c1.11 0 2.08.402 2.599 1M12 8V7m0 1v8m0 0v1m0-1c-1.11 0-2.08-.402-2.599-1M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
              </div>
            </div>
            <h3 class="text-slate-400 text-sm font-medium mb-1">Points Awarded</h3>
            <p class="text-3xl font-bold text-white">{{ analytics()!.totalPointsAwarded | number:'1.0-0' }}</p>
            <p class="text-sm text-slate-400 mt-2">Last 30 days</p>
          </div>

          <!-- Points Redeemed -->
          <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6 hover:border-purple-500/50 transition-all">
            <div class="flex items-center justify-between mb-4">
              <div class="p-3 bg-purple-500/10 rounded-lg">
                <svg class="w-6 h-6 text-purple-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v13m0-13V6a2 2 0 112 2h-2zm0 0V5.5A2.5 2.5 0 109.5 8H12zm-7 4h14M5 12a2 2 0 110-4h14a2 2 0 110 4M5 12v7a2 2 0 002 2h10a2 2 0 002-2v-7" />
                </svg>
              </div>
            </div>
            <h3 class="text-slate-400 text-sm font-medium mb-1">Points Redeemed</h3>
            <p class="text-3xl font-bold text-white">{{ analytics()!.totalPointsRedeemed | number:'1.0-0' }}</p>
            <p class="text-sm text-slate-400 mt-2">Last 30 days</p>
          </div>

          <!-- Active Campaigns -->
          <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6 hover:border-emerald-500/50 transition-all">
            <div class="flex items-center justify-between mb-4">
              <div class="p-3 bg-emerald-500/10 rounded-lg">
                <svg class="w-6 h-6 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z" />
                </svg>
              </div>
            </div>
            <h3 class="text-slate-400 text-sm font-medium mb-1">Active Campaigns</h3>
            <p class="text-3xl font-bold text-white">{{ analytics()!.activeCampaigns }}</p>
            <p class="text-sm text-emerald-400 mt-2">Running now</p>
          </div>
        </div>

        <!-- Charts Row -->
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
          <!-- Tier Distribution -->
          <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6">
            <h3 class="text-xl font-bold text-white mb-6">Customer Tier Distribution</h3>
            <div class="space-y-4">
              @for (tier of analytics()!.tierDistribution; track tier.tier) {
                <div>
                  <div class="flex items-center justify-between mb-2">
                    <span class="text-sm font-medium" [class]="getTierColor(tier.tier)">
                      {{ tier.tier }}
                    </span>
                    <span class="text-sm text-slate-400">
                      {{ tier.count }} customers ({{ tier.totalPoints | number:'1.0-0' }} pts)
                    </span>
                  </div>
                  <div class="w-full bg-slate-800 rounded-full h-2">
                    <div 
                      class="h-2 rounded-full transition-all duration-500"
                      [class]="getTierBgColor(tier.tier)"
                      [style.width.%]="getTierPercentage(tier.count)">
                    </div>
                  </div>
                </div>
              }
            </div>
          </div>

          <!-- Recent Transactions -->
          <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6">
            <h3 class="text-xl font-bold text-white mb-6">Recent Activity (7 Days)</h3>
            <div class="space-y-3">
              @for (transaction of analytics()!.recentTransactions; track transaction.date) {
                <div class="flex items-center justify-between p-3 bg-slate-800/50 rounded-lg hover:bg-slate-800 transition-colors">
                  <div>
                    <p class="text-sm font-medium text-white">
                      {{ transaction.date | date:'MMM d, y' }}
                    </p>
                    <p class="text-xs text-slate-400">
                      {{ transaction.count }} transactions
                    </p>
                  </div>
                  <div class="text-right">
                    <p class="text-sm font-bold text-amber-500">
                      +{{ transaction.totalPoints | number:'1.0-0' }}
                    </p>
                    <p class="text-xs text-slate-400">points</p>
                  </div>
                </div>
              }
            </div>
          </div>
        </div>

        <!-- Quick Actions -->
        <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6">
          <h3 class="text-xl font-bold text-white mb-6">Quick Actions</h3>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <button 
              (click)="navigateToCampaigns()"
              class="flex items-center justify-between p-4 bg-rose-700/10 border border-rose-700/30 rounded-lg hover:bg-rose-700/20 transition-all group">
              <div class="flex items-center space-x-3">
                <svg class="w-6 h-6 text-rose-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                <span class="text-white font-medium">Create Campaign</span>
              </div>
              <svg class="w-5 h-5 text-slate-400 group-hover:text-white transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>

            <button 
              (click)="navigateToRules()"
              class="flex items-center justify-between p-4 bg-amber-500/10 border border-amber-500/30 rounded-lg hover:bg-amber-500/20 transition-all group">
              <div class="flex items-center space-x-3">
                <svg class="w-6 h-6 text-amber-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                </svg>
                <span class="text-white font-medium">Add Reward Rule</span>
              </div>
              <svg class="w-5 h-5 text-slate-400 group-hover:text-white transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>

            <button 
              (click)="navigateToCustomers()"
              class="flex items-center justify-between p-4 bg-emerald-500/10 border border-emerald-500/30 rounded-lg hover:bg-emerald-500/20 transition-all group">
              <div class="flex items-center space-x-3">
                <svg class="w-6 h-6 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                </svg>
                <span class="text-white font-medium">View Customers</span>
              </div>
              <svg class="w-5 h-5 text-slate-400 group-hover:text-white transition-colors" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7" />
              </svg>
            </button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class AdminDashboardComponent implements OnInit {
  adminService = inject(AdminService);
  analytics = computed(() => this.adminService.analytics());

  ngOnInit() {
    this.loadAnalytics();
  }

  loadAnalytics() {
    this.adminService.getDashboardAnalytics().subscribe();
  }

  getTierColor(tier: string): string {
    const colors: Record<string, string> = {
      'Bronze': 'text-orange-600',
      'Silver': 'text-slate-400',
      'Gold': 'text-amber-500',
      'Platinum': 'text-purple-400'
    };
    return colors[tier] || 'text-slate-400';
  }

  getTierBgColor(tier: string): string {
    const colors: Record<string, string> = {
      'Bronze': 'bg-orange-600',
      'Silver': 'bg-slate-400',
      'Gold': 'bg-amber-500',
      'Platinum': 'bg-purple-400'
    };
    return colors[tier] || 'bg-slate-400';
  }

  getTierPercentage(count: number): number {
    const total = this.analytics()?.totalCustomers || 1;
    return (count / total) * 100;
  }

  navigateToCampaigns() {
    // TODO: Implement navigation to campaigns page
    console.log('Navigate to campaigns');
  }

  navigateToRules() {
    // TODO: Implement navigation to reward rules page
    console.log('Navigate to reward rules');
  }

  navigateToCustomers() {
    // TODO: Implement navigation to customers page
    console.log('Navigate to customers');
  }
}
