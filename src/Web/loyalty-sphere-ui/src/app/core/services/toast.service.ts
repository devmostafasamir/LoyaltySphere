import { Injectable, signal } from '@angular/core';

/**
 * Toast notification service for displaying cinematic notifications.
 * Supports success, error, warning, and info toast types.
 */
@Injectable({
  providedIn: 'root'
})
export class ToastService {
  public toasts = signal<Toast[]>([]);
  private nextId = 0;

  /**
   * Shows a success toast notification.
   */
  public success(message: string, title: string = 'Success', duration: number = 5000): void {
    this.show({
      id: this.nextId++,
      type: 'success',
      title,
      message,
      duration,
      timestamp: new Date()
    });
  }

  /**
   * Shows an error toast notification.
   */
  public error(message: string, title: string = 'Error', duration: number = 7000): void {
    this.show({
      id: this.nextId++,
      type: 'error',
      title,
      message,
      duration,
      timestamp: new Date()
    });
  }

  /**
   * Shows a warning toast notification.
   */
  public warning(message: string, title: string = 'Warning', duration: number = 5000): void {
    this.show({
      id: this.nextId++,
      type: 'warning',
      title,
      message,
      duration,
      timestamp: new Date()
    });
  }

  /**
   * Shows an info toast notification.
   */
  public info(message: string, title: string = 'Info', duration: number = 5000): void {
    this.show({
      id: this.nextId++,
      type: 'info',
      title,
      message,
      duration,
      timestamp: new Date()
    });
  }

  /**
   * Shows a reward celebration toast (special styling).
   */
  public reward(pointsAwarded: number, reason: string, duration: number = 6000): void {
    this.show({
      id: this.nextId++,
      type: 'reward',
      title: '🎉 Points Awarded!',
      message: `You earned ${pointsAwarded} points! ${reason}`,
      duration,
      timestamp: new Date()
    });
  }

  /**
   * Shows a tier upgrade celebration toast.
   */
  public tierUpgrade(newTier: string, duration: number = 8000): void {
    this.show({
      id: this.nextId++,
      type: 'celebration',
      title: '🏆 Tier Upgraded!',
      message: `Congratulations! You've reached ${newTier} tier!`,
      duration,
      timestamp: new Date()
    });
  }

  /**
   * Shows a toast notification.
   */
  private show(toast: Toast): void {
    const currentToasts = this.toasts();
    this.toasts.set([...currentToasts, toast]);

    // Auto-remove after duration
    if (toast.duration > 0) {
      setTimeout(() => this.remove(toast.id), toast.duration);
    }
  }

  /**
   * Removes a toast notification by ID.
   */
  public remove(id: number): void {
    const currentToasts = this.toasts();
    this.toasts.set(currentToasts.filter(t => t.id !== id));
  }

  /**
   * Clears all toast notifications.
   */
  public clear(): void {
    this.toasts.set([]);
  }
}

// ============================================
// Toast Types
// ============================================

export interface Toast {
  id: number;
  type: 'success' | 'error' | 'warning' | 'info' | 'reward' | 'celebration';
  title: string;
  message: string;
  duration: number;
  timestamp: Date;
}
