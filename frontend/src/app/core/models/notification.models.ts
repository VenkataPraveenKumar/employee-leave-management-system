/**
 * Visual variant categories for alert notifications.
 */
export type NotificationType = 'success' | 'error' | 'info' | 'warning';

/**
 * Encapsulates the metadata and message content of an active notification toast.
 */
export interface NotificationToast {
  /** Unique runtime identifier for tracking and dismissal. */
  id: string;
  /** Severity level dictating styling and iconography. */
  type: NotificationType;
  /** Display title or status header. */
  title: string;
  /** Explanatory message content. */
  message: string;
  /** Duration in milliseconds before self-dismissal occurs. */
  durationMs?: number;
}
