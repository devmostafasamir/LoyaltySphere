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
      <main class="flex-1 ml-64">
        <router-outlet />
      </main>
    </div>
    <app-toast />
  `
})
export class AppComponent {
  title = 'LoyaltySphere';
}
