import { Injectable, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { environment } from '../../../environments/environment';

/**
 * SignalR service for real-time reward notifications.
 * Manages WebSocket connection to RewardHub and provides reactive signals.
 */
@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection?: HubConnection;
  
  // Reactive signals for connection state
  public connectionState = signal<HubConnectionState>(HubConnectionState.Disconnected);
  public isConnected = signal<boolean>(false);
  
  // Reactive signals for notifications
  public pointsAwarded = signal<PointsAwardedNotification | null>(null);
  public pointsRedeemed = signal<PointsRedeemedNotification | null>(null);
  public tierUpgraded = signal<TierUpgradedNotification | null>(null);

  constructor() {}

  /**
   * Starts the SignalR connection to the RewardHub.
   * Automatically reconnects on disconnect.
   */
  public async startConnection(accessToken: string): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      console.log('SignalR already connected');
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/rewards`, {
        accessTokenFactory: () => accessToken,
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.ServerSentEvents
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          // Exponential backoff: 0s, 2s, 10s, 30s, then 30s
          if (retryContext.previousRetryCount === 0) return 0;
          if (retryContext.previousRetryCount === 1) return 2000;
          if (retryContext.previousRetryCount === 2) return 10000;
          return 30000;
        }
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    // Register event handlers
    this.registerEventHandlers();

    // Connection lifecycle handlers
    this.hubConnection.onreconnecting(() => {
      console.log('SignalR reconnecting...');
      this.connectionState.set(HubConnectionState.Reconnecting);
      this.isConnected.set(false);
    });

    this.hubConnection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.connectionState.set(HubConnectionState.Connected);
      this.isConnected.set(true);
    });

    this.hubConnection.onclose((error) => {
      console.error('SignalR connection closed', error);
      this.connectionState.set(HubConnectionState.Disconnected);
      this.isConnected.set(false);
    });

    try {
      await this.hubConnection.start();
      console.log('SignalR connected successfully');
      this.connectionState.set(HubConnectionState.Connected);
      this.isConnected.set(true);
    } catch (error) {
      console.error('Error starting SignalR connection', error);
      this.connectionState.set(HubConnectionState.Disconnected);
      this.isConnected.set(false);
      throw error;
    }
  }

  /**
   * Stops the SignalR connection.
   */
  public async stopConnection(): Promise<void> {
    if (this.hubConnection) {
      await this.hubConnection.stop();
      console.log('SignalR connection stopped');
      this.connectionState.set(HubConnectionState.Disconnected);
      this.isConnected.set(false);
    }
  }

  /**
   * Subscribes to updates for a specific customer.
   */
  public async subscribeToCustomer(customerId: string): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('SubscribeToCustomer', customerId);
      console.log(`Subscribed to customer: ${customerId}`);
    } else {
      console.warn('Cannot subscribe: SignalR not connected');
    }
  }

  /**
   * Unsubscribes from customer updates.
   */
  public async unsubscribeFromCustomer(customerId: string): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      await this.hubConnection.invoke('UnsubscribeFromCustomer', customerId);
      console.log(`Unsubscribed from customer: ${customerId}`);
    }
  }

  /**
   * Registers event handlers for SignalR hub events.
   */
  private registerEventHandlers(): void {
    if (!this.hubConnection) return;

    // Points Awarded Event
    this.hubConnection.on('PointsAwarded', (notification: PointsAwardedNotification) => {
      console.log('Points awarded notification received', notification);
      this.pointsAwarded.set(notification);
      
      // Auto-clear after 5 seconds
      setTimeout(() => this.pointsAwarded.set(null), 5000);
    });

    // Points Redeemed Event
    this.hubConnection.on('PointsRedeemed', (notification: PointsRedeemedNotification) => {
      console.log('Points redeemed notification received', notification);
      this.pointsRedeemed.set(notification);
      
      // Auto-clear after 5 seconds
      setTimeout(() => this.pointsRedeemed.set(null), 5000);
    });

    // Tier Upgraded Event
    this.hubConnection.on('TierUpgraded', (notification: TierUpgradedNotification) => {
      console.log('Tier upgraded notification received', notification);
      this.tierUpgraded.set(notification);
      
      // Auto-clear after 10 seconds (celebration lasts longer)
      setTimeout(() => this.tierUpgraded.set(null), 10000);
    });
  }

  /**
   * Gets the current connection state.
   */
  public getConnectionState(): HubConnectionState {
    return this.hubConnection?.state ?? HubConnectionState.Disconnected;
  }
}

// ============================================
// Notification Types
// ============================================

export interface PointsAwardedNotification {
  type: 'PointsAwarded';
  customerId: string;
  pointsAwarded: number;
  newBalance: number;
  reason: string;
  timestamp: string;
}

export interface PointsRedeemedNotification {
  type: 'PointsRedeemed';
  customerId: string;
  pointsRedeemed: number;
  newBalance: number;
  reason: string;
  timestamp: string;
}

export interface TierUpgradedNotification {
  type: 'TierUpgraded';
  customerId: string;
  previousTier: string;
  newTier: string;
  lifetimePoints: number;
  timestamp: string;
  celebration: boolean;
}
