import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';

/**
 * Navigation component for switching between customer and admin views.
 * Provides a cinematic sidebar navigation with smooth transitions.
 */
@Component({
  selector: 'app-navigation',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <!-- Mobile Header (Visible only on small screens) -->
    <div class="md:hidden fixed top-0 left-0 right-0 h-16 bg-slate-900/95 backdrop-blur-md border-b border-slate-800 z-50 px-4 flex items-center justify-between">
      <h1 class="text-xl font-bold text-gradient-cinematic leading-none">LoyaltySphere</h1>
      <button 
        (click)="toggleSidebar()"
        class="p-2 text-slate-400 hover:text-white transition-colors relative z-[60]">
        <svg class="w-6 h-6" [class.hidden]="isSidebarOpen()" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16m-7 6h7" />
        </svg>
        <svg class="w-6 h-6" [class.hidden]="!isSidebarOpen()" fill="none" stroke="currentColor" viewBox="0 0 24 24">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
        </svg>
      </button>
    </div>

    <!-- Backdrop overlay for mobile -->
    @if (isSidebarOpen()) {
      <div 
        (click)="closeSidebar()"
        class="md:hidden fixed inset-0 bg-black/60 backdrop-blur-sm z-40 animate-fade-in">
      </div>
    }

    <!-- Sidebar -->
    <nav 
      [class.translate-x-0]="isSidebarOpen()"
      [class.-translate-x-full]="!isSidebarOpen()"
      class="fixed md:translate-x-0 top-0 left-0 h-full w-64 bg-slate-900/95 backdrop-blur-sm border-r border-slate-800 z-50 flex flex-col transition-transform duration-300 md:z-40">
      
      <!-- Logo -->
      <div class="p-6 border-b border-slate-800">
        <h1 class="text-2xl font-bold text-white flex items-center space-x-2">
          <svg class="w-8 h-8 text-crimson-600" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
          </svg>
          <span class="text-gradient-cinematic">
            LoyaltySphere
          </span>
        </h1>
      </div>

      <!-- Navigation Content -->
      <div class="flex-1 overflow-y-auto py-4">
        <!-- Customer Section -->
        <div class="px-4 mb-6">
          <p class="text-[10px] font-bold text-slate-500 uppercase tracking-[0.2em] mb-4 px-4">Customer Experience</p>
          <a 
            routerLink="/dashboard"
            routerLinkActive="bg-crimson-800/10 border-crimson-800 text-white shadow-glow"
            (click)="closeSidebar()"
            class="flex items-center space-x-3 px-4 py-3 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent group">
            <svg class="w-5 h-5 transition-transform group-hover:scale-110" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
            </svg>
            <span class="font-semibold text-sm">Main Dashboard</span>
          </a>
        </div>

        <!-- Admin Section -->
        <div class="px-4 border-t border-slate-800 pt-6">
          <p class="text-[10px] font-bold text-slate-500 uppercase tracking-[0.2em] mb-4 px-4">Management</p>
          <div class="space-y-1">
            <a 
              routerLink="/admin/dashboard"
              routerLinkActive="bg-crimson-800/10 border-crimson-800 text-white shadow-glow"
              (click)="closeSidebar()"
              class="flex items-center space-x-3 px-4 py-3 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent group">
              <svg class="w-5 h-5 transition-transform group-hover:scale-110" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
              </svg>
              <span class="font-semibold text-sm">Analytics Hub</span>
            </a>

            <a 
              routerLink="/admin/campaigns"
              routerLinkActive="bg-crimson-800/10 border-crimson-800 text-white shadow-glow"
              (click)="closeSidebar()"
              class="flex items-center space-x-3 px-4 py-3 rounded-xl text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent group">
              <svg class="w-5 h-5 transition-transform group-hover:scale-110" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z" />
              </svg>
              <span class="font-semibold text-sm">Active Campaigns</span>
            </a>
          </div>
        </div>
      </div>

      <!-- Footer User Profile -->
      <div class="p-4 border-t border-slate-800">
        <div class="flex items-center space-x-3 px-4 py-3 bg-slate-800/30 rounded-2xl border border-slate-700/50 hover:bg-slate-800/50 transition-colors group cursor-pointer">
          <div class="relative">
            <div class="w-10 h-10 bg-gradient-to-br from-crimson-700 to-gold-500 rounded-lg flex items-center justify-center shadow-lg group-hover:scale-105 transition-transform">
              <span class="text-white font-black text-xs tracking-tighter">AD</span>
            </div>
            <div class="absolute -bottom-0.5 -right-0.5 w-3 h-3 bg-emerald-500 border-2 border-slate-900 rounded-full"></div>
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-bold text-white truncate">Administrator</p>
            <p class="text-[10px] text-slate-500 truncate font-medium">Internal Access</p>
          </div>
        </div>
      </div>
    </nav>
  `,
  styles: []
})
export class NavigationComponent {
  isSidebarOpen = signal(false);

  toggleSidebar() {
    this.isSidebarOpen.update(v => !v);
  }

  closeSidebar() {
    this.isSidebarOpen.set(false);
  }
}
