import { Component, OnInit, OnDestroy, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SignalRService } from '../../core/services/signalr.service';
import { RewardService } from '../../core/services/reward.service';
import { ToastService } from '../../core/services/toast.service';

/**
 * Main dashboard component with real-time points display.
 * Shows customer balance, tier, and live transaction feed.
 * Integrates with SignalR for real-time updates.
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-h-screen bg-slate-950 text-white">
      <!-- Header -->
      <header class="border-b border-slate-800 bg-slate-900/50 backdrop-blur-sm">
        <div class="container mx-auto px-4 py-6">
          <div class="flex items-center justify-between">
            <div>
              <h1 class="text-3xl font-bold text-crimson-500">LoyaltySphere</h1>
              <p class="text-slate-400 text-sm mt-1">Your Rewards Dashboard</p>
            </div>
            <div class="flex items-center gap-4">
              <!-- Connection Status -->
              <div class="flex items-center gap-2">
                <div 
                  class="w-2 h-2 rounded-full"
                  [class.bg-emerald-500]="signalRService.isConnected()"
                  [class.bg-red-500]="!signalRService.isConnected()"
                  [class.animate-pulse]="!signalRService.isConnected()">
                </div>
                <span class="text-sm text-slate-400">
                  {{ signalRService.isConnected() ? 'Connected' : 'Disconnected' }}
                </span>
              </div>
              
              <!-- Tier Badge -->
              <div class="px-4 py-2 rounded-full bg-gradient-to-r from-gold-500 to-gold-600 text-slate-900 font-semibold text-sm">
                {{ rewardService.currentTier() }}
              </div>
            </div>
          </div>
        </div>
      </header>

      <!-- Main Content -->
      <main class="container mx-auto px-4 py-8">
        <!-- Points Balance Card -->
        <div class="glass-card p-8 mb-8 relative overflow-hidden">
          <!-- Background Glow -->
          <div class="absolute inset-0 bg-gradient-to-br from-crimson-500/10 to-transparent pointer-events-none"></div>
          
          <div class="relative z-10">
            <p class="text-slate-400 text-sm uppercase tracking-wider mb-2">Your Points Balance</p>
            <div class="flex items-baseline gap-4">
              <h2 
                class="text-6xl font-bold text-crimson-500 transition-all duration-500"
                [class.animate-scaleIn]="balanceAnimating()">
                {{ rewardService.currentBalance().toLocaleString() }}
              </h2>
              <span class="text-2xl text-slate-400">pts</span>
            </div>
            
            <!-- Lifetime Points -->
            <div class="mt-4 flex items-center gap-6">
              <div>
                <p class="text-slate-500 text-xs uppercase">Lifetime Points</p>
                <p class="text-slate-300 text-lg font-semibold">
                  {{ rewardService.lifetimePoints().toLocaleString() }}
                </p>
              </div>
              
              <!-- Progress to Next Tier -->
              <div class="flex-1">
                <div class="flex items-center justify-between mb-1">
                  <p class="text-slate-500 text-xs uppercase">Progress to Next Tier</p>
                  <p class="text-slate-400 text-xs">{{ rewardService.progressToNextTier() }}%</p>
                </div>
                <div class="h-2 bg-slate-800 rounded-full overflow-hidden">
                  <div 
                    class="h-full bg-gradient-to-r from-crimson-500 to-gold-500 transition-all duration-1000 ease-out"
                    [style.width.%]="rewardService.progressToNextTier()">
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Recent Transactions -->
        <div class="glass-card p-6">
          <h3 class="text-xl font-bold mb-4 flex items-center gap-2">
            <svg class="w-5 h-5 text-crimson-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path>
            </svg>
            Recent Activity
          </h3>
          
          <div class="space-y-3">
            @if (recentTransactions().length === 0) {
              <div class="text-center py-12 text-slate-500">
                <svg class="w-16 h-16 mx-auto mb-4 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4"></path>
                </svg>
                <p>No recent transactions</p>
              </div>
            } @else {
              @for (transaction of recentTransactions(); track transaction.id) {
                <div 
                  class="flex items-center justify-between p-4 rounded-lg bg-slate-900/50 border border-slate-800 hover:border-crimson-500/50 transition-all duration-300 animate-fadeIn">
                  <div class="flex items-center gap-4">
                    <!-- Icon -->
                    <div 
                      class="w-10 h-10 rounded-full flex items-center justify-center"
                      [class.bg-emerald-500/20]="transaction.type === 'Earned'"
                      [class.bg-crimson-500/20]="transaction.type === 'Redeemed'">
                      @if (transaction.type === 'Earned') {
                        <svg class="w-5 h-5 text-emerald-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 4v16m8-8H4"></path>
                        </svg>
                      } @else {
                        <svg class="w-5 h-5 text-crimson-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H4"></path>
                        </svg>
                      }
                    </div>
                    
                    <!-- Details -->
                    <div>
                      <p class="font-semibold">{{ transaction.description }}</p>
                      <p class="text-sm text-slate-400">{{ transaction.timestamp | date:'short' }}</p>
                    </div>
                  </div>
                  
                  <!-- Points -->
                  <div class="text-right">
                    <p 
                      class="text-xl font-bold"
                      [class.text-emerald-500]="transaction.type === 'Earned'"
                      [class.text-crimson-500]="transaction.type === 'Redeemed'">
                      {{ transaction.type === 'Earned' ? '+' : '-' }}{{ transaction.points.toLocaleString() }}
                    </p>
                    <p class="text-xs text-slate-500">{{ transaction.type }}</p>
                  </div>
                </div>
              }
            }
          </div>
        </div>
      </main>

      <!-- Reward Animation Overlay -->
      @if (showRewardAnimation()) {
        <div class="fixed inset-0 pointer-events-none z-50 flex items-center justify-center">
          <div class="animate-rewardPop">
            <div class="text-center">
              <div class="text-8xl mb-4 animate-sparkle">✨</div>
              <div class="text-6xl font-bold text-gold-500 animate-scaleIn">
                +{{ lastPointsAwarded() }}
              </div>
              <p class="text-2xl text-white mt-2">Points Earned!</p>
            </div>
          </div>
        </div>
      }

      <!-- Tier Upgrade Celebration -->
      @if (showTierCelebration()) {
        <div class="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center animate-fadeIn">
          <div class="text-center animate-scaleIn">
            <div class="text-9xl mb-6 animate-sparkle">🏆</div>
            <h2 class="text-6xl font-bold text-gold-500 mb-4">Congratulations!</h2>
            <p class="text-3xl text-white mb-2">You've reached</p>
            <p class="text-5xl font-bold text-crimson-500">{{ newTier() }} Tier!</p>
            <button 
              (click)="closeTierCelebration()"
              class="mt-8 px-8 py-3 bg-gradient-to-r from-crimson-500 to-crimson-600 rounded-full font-semibold hover:shadow-glow transition-all duration-300">
              Continue
            </button>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .glass-card {
      @apply bg-slate-900/30 backdrop-blur-sm border border-slate-800 rounded-2xl;
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.3);
    }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  // Services
  public signalRService = this.signalR;
  public rewardService = this.rewards;

  // Component state
  public recentTransactions = signal<Transaction[]>([]);
  public balanceAnimating = signal(false);
  public showRewardAnimation = signal(false);
  public showTierCelebration = signal(false);
  public lastPointsAwarded = signal(0);
  public newTier = signal('');

  // Mock customer ID (in real app, get from auth service)
  private customerId = 'cust-123';

  constructor(
    private signalR: SignalRService,
    private rewards: RewardService,
    private toast: ToastService
  ) {
    // React to SignalR notifications
    effect(() => {
      const notification = this.signalR.pointsAwarded();
      if (notification) {
        this.handlePointsAwarded(notification);
      }
    });

    effect(() => {
      const notification = this.signalR.tierUpgraded();
      if (notification) {
        this.handleTierUpgraded(notification);
      }
    });
  }

  async ngOnInit() {
    // Load initial balance
    this.rewards.getBalance(this.customerId).subscribe({
      next: () => console.log('Balance loaded'),
      error: (err) => console.error('Error loading balance', err)
    });

    // Connect to SignalR (in real app, get token from auth service)
    const mockToken = 'mock-jwt-token';
    try {
      await this.signalR.startConnection(mockToken);
      await this.signalR.subscribeToCustomer(this.customerId);
    } catch (error) {
      console.error('SignalR connection failed', error);
      this.toast.error('Failed to connect to real-time updates');
    }
  }

  ngOnDestroy() {
    this.signalR.stopConnection();
  }

  private handlePointsAwarded(notification: any): void {
    // Update balance
    this.rewards.updateBalanceFromNotification(notification.newBalance);
    
    // Animate balance
    this.balanceAnimating.set(true);
    setTimeout(() => this.balanceAnimating.set(false), 500);

    // Show reward animation
    this.lastPointsAwarded.set(notification.pointsAwarded);
    this.showRewardAnimation.set(true);
    setTimeout(() => this.showRewardAnimation.set(false), 3000);

    // Add to transaction feed
    const transaction: Transaction = {
      id: Date.now().toString(),
      type: 'Earned',
      points: notification.pointsAwarded,
      description: notification.reason,
      timestamp: new Date(notification.timestamp)
    };
    this.recentTransactions.update(txs => [transaction, ...txs].slice(0, 10));

    // Show toast
    this.toast.reward(notification.pointsAwarded, notification.reason);
  }

  private handleTierUpgraded(notification: any): void {
    // Update tier
    this.rewards.updateTierFromNotification(notification.newTier);
    
    // Show celebration
    this.newTier.set(notification.newTier);
    this.showTierCelebration.set(true);

    // Show toast
    this.toast.tierUpgrade(notification.newTier);
  }

  public closeTierCelebration(): void {
    this.showTierCelebration.set(false);
  }
}

interface Transaction {
  id: string;
  type: 'Earned' | 'Redeemed';
  points: number;
  description: string;
  timestamp: Date;
}
