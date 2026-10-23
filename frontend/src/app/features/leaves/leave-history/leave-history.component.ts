import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { LeaveService } from '../../../core/services/leave.service';
import { LeaveRequestSummary } from '../../../core/models/leave.models';

/**
 * Component displaying the comprehensive audit log of leave applications submitted by the user.
 */
@Component({
  selector: 'app-leave-history',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './leave-history.component.html',
  styleUrls: ['./leave-history.component.scss']
})
export class LeaveHistoryComponent implements OnInit {
  /** Signal containing the collection of user leave applications. */
  public requests = signal<LeaveRequestSummary[]>([]);
  /** Loading state indicator. */
  public isLoading = signal<boolean>(true);
  /** Error message signal. */
  public errorMessage = signal<string | null>(null);

  /**
   * Initializes the leave service dependency.
   * @param leaveService Service handling leave API communication.
   */
  constructor(private leaveService: LeaveService) {}

  /**
   * Angular lifecycle hook executed on initialization to fetch history.
   */
  public ngOnInit(): void {
    this.fetchRequests();
  }

  /**
   * Retrieves all leave booking records submitted by the authenticated employee.
   */
  public fetchRequests(): void {
    this.isLoading.set(true);
    this.leaveService.getMyRequests().subscribe({
      next: (data) => {
        this.requests.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.detail || 'Failed to load leave history.');
        this.isLoading.set(false);
      }
    });
  }
}
