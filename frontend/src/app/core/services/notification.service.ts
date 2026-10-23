import { Injectable, signal } from '@angular/core';
import { NotificationToast, NotificationType } from '../models/notification.models';

/**
 * Central reactive service managing system-wide toast notification alerts.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  /** Reactive signal containing the list of currently visible notifications. */
  public toasts = signal<NotificationToast[]>([]);

  /**
   * Pushes a success alert to the notification queue.
   * @param message Explanatory text.
   * @param title Header title (defaults to 'Success').
   */
  public showSuccess(message: string, title = 'Success'): void {
    this.addToast('success', title, message);
  }

  /**
   * Pushes an error alert to the notification queue.
   * @param message Detailed error description.
   * @param title Header title (defaults to 'Error').
   */
  public showError(message: string, title = 'Error'): void {
    this.addToast('error', title, message, 6000);
  }

  /**
   * Pushes an informational alert to the notification queue.
   * @param message Informational text.
   * @param title Header title (defaults to 'Notice').
   */
  public showInfo(message: string, title = 'Notice'): void {
    this.addToast('info', title, message);
  }

  /**
   * Removes a notification toast from the queue by its identifier.
   * @param id The unique identifier of the toast to dismiss.
   */
  public dismiss(id: string): void {
    this.toasts.update((current) => current.filter((t) => t.id !== id));
  }

  /**
   * Appends a new toast notification and configures automatic dismissal timers.
   * @param type Notification severity variant.
   * @param title Header title.
   * @param message Body text.
   * @param durationMs Milliseconds before self-dismissal.
   */
  private addToast(
    type: NotificationType,
    title: string,
    message: string,
    durationMs = 4000
  ): void {
    const id = `${Date.now()}-${Math.random().toString(36).substring(2, 9)}`;
    const toast: NotificationToast = { id, type, title, message, durationMs };

    this.toasts.update((current) => [...current, toast]);

    if (durationMs > 0) {
      setTimeout(() => {
        this.dismiss(id);
      }, durationMs);
    }
  }
}
