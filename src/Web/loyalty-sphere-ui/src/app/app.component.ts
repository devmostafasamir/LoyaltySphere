import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ToastComponent } from './shared/components/toast/toast.component';
import { NavigationComponent } from './shared/navigation.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent, NavigationComponent],
  template: `
    <div class="flex min-h-screen bg-slate-950">
      <app-navigation />
      <!-- Content Area: Responsive margins to account for Sidebar (desktop) and Header (mobile) -->
      <main class="flex-1 md:ml-64 pt-16 md:pt-0 min-w-0">
        <router-outlet />
      </main>
    </div>
    <app-toast />
  `
})
export class AppComponent {
  title = 'LoyaltySphere';
}
