import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';

/**
 * Shared container rendering floating toast alerts dispatched by the NotificationService.
 */
@Component({
  selector: 'app-toast-container',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './toast-container.component.html',
  styleUrls: ['./toast-container.component.scss']
})
export class ToastContainerComponent {
  /**
   * Injects the notification service to access reactive toast signals.
   * @param notificationService Service holding current alerts.
   */
  constructor(public notificationService: NotificationService) {}
}
