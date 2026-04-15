import { Component } from '@angular/core';
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
    <nav class="fixed top-0 left-0 h-full w-64 bg-slate-900/95 backdrop-blur-sm border-r border-slate-800 z-40">
      <!-- Logo -->
      <div class="p-6 border-b border-slate-800">
        <h1 class="text-2xl font-bold text-white flex items-center space-x-2">
          <svg class="w-8 h-8 text-rose-700" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 3v4M3 5h4M6 17v4m-2-2h4m5-16l2.286 6.857L21 12l-5.714 2.143L13 21l-2.286-6.857L5 12l5.714-2.143L13 3z" />
          </svg>
          <span class="bg-gradient-to-r from-rose-700 to-amber-500 bg-clip-text text-transparent">
            LoyaltySphere
          </span>
        </h1>
      </div>

      <!-- Customer Section -->
      <div class="p-4">
        <p class="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3">Customer</p>
        <a 
          routerLink="/dashboard"
          routerLinkActive="bg-rose-700/20 border-rose-700 text-white"
          class="flex items-center space-x-3 px-4 py-3 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent">
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6" />
          </svg>
          <span class="font-medium">Dashboard</span>
        </a>
      </div>

      <!-- Admin Section -->
      <div class="p-4 border-t border-slate-800">
        <p class="text-xs font-semibold text-slate-500 uppercase tracking-wider mb-3">Admin</p>
        <div class="space-y-1">
          <a 
            routerLink="/admin/dashboard"
            routerLinkActive="bg-rose-700/20 border-rose-700 text-white"
            class="flex items-center space-x-3 px-4 py-3 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
            <span class="font-medium">Analytics</span>
          </a>

          <a 
            routerLink="/admin/campaigns"
            routerLinkActive="bg-rose-700/20 border-rose-700 text-white"
            class="flex items-center space-x-3 px-4 py-3 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4M7.835 4.697a3.42 3.42 0 001.946-.806 3.42 3.42 0 014.438 0 3.42 3.42 0 001.946.806 3.42 3.42 0 013.138 3.138 3.42 3.42 0 00.806 1.946 3.42 3.42 0 010 4.438 3.42 3.42 0 00-.806 1.946 3.42 3.42 0 01-3.138 3.138 3.42 3.42 0 00-1.946.806 3.42 3.42 0 01-4.438 0 3.42 3.42 0 00-1.946-.806 3.42 3.42 0 01-3.138-3.138 3.42 3.42 0 00-.806-1.946 3.42 3.42 0 010-4.438 3.42 3.42 0 00.806-1.946 3.42 3.42 0 013.138-3.138z" />
            </svg>
            <span class="font-medium">Campaigns</span>
          </a>

          <a 
            routerLink="/admin/customers"
            routerLinkActive="bg-rose-700/20 border-rose-700 text-white"
            class="flex items-center space-x-3 px-4 py-3 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
            </svg>
            <span class="font-medium">Customers</span>
          </a>

          <a 
            routerLink="/admin/rules"
            routerLinkActive="bg-rose-700/20 border-rose-700 text-white"
            class="flex items-center space-x-3 px-4 py-3 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-all border border-transparent">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 6V4m0 2a2 2 0 100 4m0-4a2 2 0 110 4m-6 8a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4m6 6v10m6-2a2 2 0 100-4m0 4a2 2 0 110-4m0 4v2m0-6V4" />
            </svg>
            <span class="font-medium">Reward Rules</span>
          </a>
        </div>
      </div>

      <!-- Footer -->
      <div class="absolute bottom-0 left-0 right-0 p-4 border-t border-slate-800">
        <div class="flex items-center space-x-3 px-4 py-3">
          <div class="w-10 h-10 bg-gradient-to-br from-rose-700 to-amber-500 rounded-full flex items-center justify-center">
            <span class="text-white font-bold text-sm">AD</span>
          </div>
          <div class="flex-1">
            <p class="text-sm font-medium text-white">Admin User</p>
            <p class="text-xs text-slate-400">admin&#64;loyaltysphere.com</p>
          </div>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    :host {
      display: block;
    }
  `]
})
export class NavigationComponent {}
