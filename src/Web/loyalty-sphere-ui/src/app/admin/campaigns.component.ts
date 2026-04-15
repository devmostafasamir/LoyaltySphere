import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService, Campaign, CreateCampaignRequest } from '../core/services/admin.service';
import { ToastService } from '../core/services/toast.service';

/**
 * Campaigns management component for creating and managing marketing campaigns.
 * Supports Bonus, Multiplier, and Cashback campaign types.
 */
@Component({
  selector: 'app-campaigns',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="min-h-screen bg-slate-950 p-6">
      <!-- Header -->
      <div class="flex items-center justify-between mb-8">
        <div>
          <h1 class="text-3xl font-bold text-white mb-2">Campaigns</h1>
          <p class="text-slate-400">Create and manage marketing campaigns</p>
        </div>
        <button 
          (click)="showCreateModal = true"
          class="px-6 py-3 bg-rose-700 hover:bg-rose-600 text-white font-medium rounded-lg transition-colors flex items-center space-x-2">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
          </svg>
          <span>Create Campaign</span>
        </button>
      </div>

      <!-- Filters -->
      <div class="mb-6 flex items-center space-x-4">
        <button 
          (click)="filterActive = null; loadCampaigns()"
          [class.bg-rose-700]="filterActive === null"
          [class.bg-slate-800]="filterActive !== null"
          class="px-4 py-2 rounded-lg text-white font-medium transition-colors">
          All
        </button>
        <button 
          (click)="filterActive = true; loadCampaigns()"
          [class.bg-rose-700]="filterActive === true"
          [class.bg-slate-800]="filterActive !== true"
          class="px-4 py-2 rounded-lg text-white font-medium transition-colors">
          Active
        </button>
        <button 
          (click)="filterActive = false; loadCampaigns()"
          [class.bg-rose-700]="filterActive === false"
          [class.bg-slate-800]="filterActive !== false"
          class="px-4 py-2 rounded-lg text-white font-medium transition-colors">
          Inactive
        </button>
      </div>

      <!-- Loading State -->
      @if (adminService.loading()) {
        <div class="flex items-center justify-center h-64">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-rose-700"></div>
        </div>
      }

      <!-- Campaigns Grid -->
      @if (!adminService.loading()) {
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          @for (campaign of campaigns(); track campaign.id) {
            <div class="bg-slate-900/50 backdrop-blur-sm border border-slate-800 rounded-xl p-6 hover:border-rose-700/50 transition-all">
              <!-- Campaign Header -->
              <div class="flex items-start justify-between mb-4">
                <div class="flex-1">
                  <h3 class="text-lg font-bold text-white mb-1">{{ campaign.campaignName }}</h3>
                  <span 
                    class="inline-block px-2 py-1 text-xs font-medium rounded"
                    [class]="getCampaignTypeBadge(campaign.campaignType)">
                    {{ campaign.campaignType }}
                  </span>
                </div>
                <button 
                  (click)="toggleCampaignStatus(campaign)"
                  [class.bg-emerald-500]="campaign.isActive"
                  [class.bg-slate-700]="!campaign.isActive"
                  class="px-3 py-1 rounded-lg text-xs font-medium text-white transition-colors">
                  {{ campaign.isActive ? 'Active' : 'Inactive' }}
                </button>
              </div>

              <!-- Campaign Details -->
              <p class="text-sm text-slate-400 mb-4">{{ campaign.description }}</p>

              <!-- Campaign Stats -->
              <div class="space-y-2 mb-4">
                @if (campaign.campaignType === 'Bonus') {
                  <div class="flex items-center justify-between text-sm">
                    <span class="text-slate-400">Bonus Points:</span>
                    <span class="text-amber-500 font-bold">+{{ campaign.bonusPoints }}</span>
                  </div>
                }
                @if (campaign.campaignType === 'Multiplier') {
                  <div class="flex items-center justify-between text-sm">
                    <span class="text-slate-400">Multiplier:</span>
                    <span class="text-amber-500 font-bold">{{ campaign.pointsMultiplier }}x</span>
                  </div>
                }
                @if (campaign.campaignType === 'Cashback') {
                  <div class="flex items-center justify-between text-sm">
                    <span class="text-slate-400">Cashback:</span>
                    <span class="text-amber-500 font-bold">{{ campaign.cashbackPercentage }}%</span>
                  </div>
                }
                <div class="flex items-center justify-between text-sm">
                  <span class="text-slate-400">Duration:</span>
                  <span class="text-white">{{ campaign.startDate | date:'MMM d' }} - {{ campaign.endDate | date:'MMM d, y' }}</span>
                </div>
                @if (campaign.maxParticipations) {
                  <div class="flex items-center justify-between text-sm">
                    <span class="text-slate-400">Participations:</span>
                    <span class="text-white">{{ campaign.currentParticipations }} / {{ campaign.maxParticipations }}</span>
                  </div>
                }
              </div>

              <!-- Campaign Filters -->
              @if (campaign.targetCustomerSegment || campaign.targetMerchantCategory) {
                <div class="pt-4 border-t border-slate-800">
                  <p class="text-xs text-slate-500 mb-2">Targeting:</p>
                  <div class="flex flex-wrap gap-2">
                    @if (campaign.targetCustomerSegment) {
                      <span class="px-2 py-1 bg-purple-500/10 text-purple-400 text-xs rounded">
                        {{ campaign.targetCustomerSegment }} Tier
                      </span>
                    }
                    @if (campaign.targetMerchantCategory) {
                      <span class="px-2 py-1 bg-blue-500/10 text-blue-400 text-xs rounded">
                        {{ campaign.targetMerchantCategory }}
                      </span>
                    }
                  </div>
                </div>
              }
            </div>
          }
        </div>

        @if (campaigns().length === 0) {
          <div class="text-center py-12">
            <svg class="w-16 h-16 text-slate-700 mx-auto mb-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z" />
            </svg>
            <p class="text-slate-400">No campaigns found</p>
            <button 
              (click)="showCreateModal = true"
              class="mt-4 px-6 py-2 bg-rose-700 hover:bg-rose-600 text-white rounded-lg transition-colors">
              Create Your First Campaign
            </button>
          </div>
        }
      }

      <!-- Create Campaign Modal -->
      @if (showCreateModal) {
        <div class="fixed inset-0 bg-black/80 backdrop-blur-sm flex items-center justify-center z-50 p-4">
          <div class="bg-slate-900 border border-slate-800 rounded-xl max-w-2xl w-full max-h-[90vh] overflow-y-auto">
            <div class="p-6 border-b border-slate-800">
              <h2 class="text-2xl font-bold text-white">Create Campaign</h2>
            </div>

            <form (ngSubmit)="createCampaign()" class="p-6 space-y-4">
              <!-- Campaign Name -->
              <div>
                <label class="block text-sm font-medium text-slate-300 mb-2">Campaign Name</label>
                <input 
                  type="text" 
                  [(ngModel)]="newCampaign.campaignName"
                  name="campaignName"
                  required
                  class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700"
                  placeholder="Summer Bonus Campaign">
              </div>

              <!-- Description -->
              <div>
                <label class="block text-sm font-medium text-slate-300 mb-2">Description</label>
                <textarea 
                  [(ngModel)]="newCampaign.description"
                  name="description"
                  required
                  rows="3"
                  class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700"
                  placeholder="Earn bonus points on all purchases"></textarea>
              </div>

              <!-- Campaign Type -->
              <div>
                <label class="block text-sm font-medium text-slate-300 mb-2">Campaign Type</label>
                <select 
                  [(ngModel)]="newCampaign.campaignType"
                  name="campaignType"
                  required
                  class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
                  <option value="Bonus">Bonus Points</option>
                  <option value="Multiplier">Points Multiplier</option>
                  <option value="Cashback">Cashback</option>
                </select>
              </div>

              <!-- Type-Specific Fields -->
              @if (newCampaign.campaignType === 'Bonus') {
                <div>
                  <label class="block text-sm font-medium text-slate-300 mb-2">Bonus Points</label>
                  <input 
                    type="number" 
                    [(ngModel)]="newCampaign.bonusPoints"
                    name="bonusPoints"
                    required
                    min="1"
                    class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
                </div>
              }

              @if (newCampaign.campaignType === 'Multiplier') {
                <div>
                  <label class="block text-sm font-medium text-slate-300 mb-2">Points Multiplier</label>
                  <input 
                    type="number" 
                    [(ngModel)]="newCampaign.pointsMultiplier"
                    name="pointsMultiplier"
                    required
                    min="1.1"
                    step="0.1"
                    class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
                </div>
              }

              @if (newCampaign.campaignType === 'Cashback') {
                <div>
                  <label class="block text-sm font-medium text-slate-300 mb-2">Cashback Percentage</label>
                  <input 
                    type="number" 
                    [(ngModel)]="newCampaign.cashbackPercentage"
                    name="cashbackPercentage"
                    required
                    min="0.1"
                    max="100"
                    step="0.1"
                    class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
                </div>
              }

              <!-- Date Range -->
              <div class="grid grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-slate-300 mb-2">Start Date</label>
                  <input 
                    type="date" 
                    [(ngModel)]="newCampaign.startDate"
                    name="startDate"
                    required
                    class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
                </div>
                <div>
                  <label class="block text-sm font-medium text-slate-300 mb-2">End Date</label>
                  <input 
                    type="date" 
                    [(ngModel)]="newCampaign.endDate"
                    name="endDate"
                    required
                    class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
                </div>
              </div>

              <!-- Optional Fields -->
              <div>
                <label class="block text-sm font-medium text-slate-300 mb-2">Target Customer Segment (Optional)</label>
                <select 
                  [(ngModel)]="newCampaign.targetCustomerSegment"
                  name="targetCustomerSegment"
                  class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
                  <option value="">All Customers</option>
                  <option value="Bronze">Bronze</option>
                  <option value="Silver">Silver</option>
                  <option value="Gold">Gold</option>
                  <option value="Platinum">Platinum</option>
                </select>
              </div>

              <div>
                <label class="block text-sm font-medium text-slate-300 mb-2">Minimum Transaction Amount (Optional)</label>
                <input 
                  type="number" 
                  [(ngModel)]="newCampaign.minimumTransactionAmount"
                  name="minimumTransactionAmount"
                  min="0"
                  step="0.01"
                  class="w-full px-4 py-2 bg-slate-800 border border-slate-700 rounded-lg text-white focus:outline-none focus:border-rose-700">
              </div>

              <!-- Actions -->
              <div class="flex items-center justify-end space-x-4 pt-4">
                <button 
                  type="button"
                  (click)="closeCreateModal()"
                  class="px-6 py-2 bg-slate-800 hover:bg-slate-700 text-white rounded-lg transition-colors">
                  Cancel
                </button>
                <button 
                  type="submit"
                  class="px-6 py-2 bg-rose-700 hover:bg-rose-600 text-white rounded-lg transition-colors">
                  Create Campaign
                </button>
              </div>
            </form>
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
export class CampaignsComponent implements OnInit {
  adminService = inject(AdminService);
  toastService = inject(ToastService);

  campaigns = computed(() => this.adminService.campaigns());
  showCreateModal = false;
  filterActive: boolean | null = null;

  newCampaign: Partial<CreateCampaignRequest> = {
    campaignType: 'Bonus'
  };

  ngOnInit() {
    this.loadCampaigns();
  }

  loadCampaigns() {
    this.adminService.getCampaigns(this.filterActive ?? undefined).subscribe();
  }

  createCampaign() {
    if (!this.isValidCampaign()) {
      this.toastService.error('Please fill in all required fields');
      return;
    }

    this.adminService.createCampaign(this.newCampaign as CreateCampaignRequest).subscribe({
      next: () => {
        this.toastService.success('Campaign created successfully!');
        this.closeCreateModal();
      },
      error: (error) => {
        this.toastService.error(error.error?.detail || 'Failed to create campaign');
      }
    });
  }

  toggleCampaignStatus(campaign: Campaign) {
    const action = campaign.isActive 
      ? this.adminService.deactivateCampaign(campaign.id)
      : this.adminService.activateCampaign(campaign.id);

    action.subscribe({
      next: () => {
        const status = campaign.isActive ? 'deactivated' : 'activated';
        this.toastService.success(`Campaign ${status} successfully!`);
      },
      error: (error) => {
        this.toastService.error(error.error?.detail || 'Failed to update campaign');
      }
    });
  }

  closeCreateModal() {
    this.showCreateModal = false;
    this.newCampaign = { campaignType: 'Bonus' };
  }

  isValidCampaign(): boolean {
    return !!(
      this.newCampaign.campaignName &&
      this.newCampaign.description &&
      this.newCampaign.campaignType &&
      this.newCampaign.startDate &&
      this.newCampaign.endDate
    );
  }

  getCampaignTypeBadge(type: string): string {
    const badges: Record<string, string> = {
      'Bonus': 'bg-amber-500/10 text-amber-500',
      'Multiplier': 'bg-purple-500/10 text-purple-400',
      'Cashback': 'bg-emerald-500/10 text-emerald-400'
    };
    return badges[type] || 'bg-slate-700 text-slate-300';
  }
}
