import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';

/**
 * LoyaltySphere Angular Application Bootstrap
 * 
 * Standalone component architecture with:
 * - Signals for reactive state management
 * - SignalR for real-time updates
 * - Tailwind v4 cinematic red theme
 * - OnPush change detection for performance
 */
bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
