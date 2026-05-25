import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface Notification {
  id: string;
  type: 'success' | 'error' | 'warning' | 'info';
  message: string;
  duration?: number; // ms, default 3500
}

/**
 * Centralized notification service for consistent messaging across the app.
 * Automatically dismisses toasts after a timeout.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly notifications$ = new BehaviorSubject<Notification[]>([]);
  private readonly defaultDuration = 3500;

  getNotifications(): Observable<Notification[]> {
    return this.notifications$.asObservable();
  }

  success(message: string, duration = this.defaultDuration): void {
    this.addNotification('success', message, duration);
  }

  error(message: string, duration = this.defaultDuration): void {
    this.addNotification('error', message, duration);
  }

  warning(message: string, duration = this.defaultDuration): void {
    this.addNotification('warning', message, duration);
  }

  info(message: string, duration = this.defaultDuration): void {
    this.addNotification('info', message, duration);
  }

  dismiss(notificationId: string): void {
    const current = this.notifications$.value;
    this.notifications$.next(current.filter((n) => n.id !== notificationId));
  }

  private addNotification(type: Notification['type'], message: string, duration: number): void {
    const id = `notification-${Date.now()}-${Math.random()}`;
    const notification: Notification = { id, type, message, duration };

    const current = this.notifications$.value;
    this.notifications$.next([...current, notification]);

    if (duration > 0) {
      setTimeout(() => this.dismiss(id), duration);
    }
  }
}
