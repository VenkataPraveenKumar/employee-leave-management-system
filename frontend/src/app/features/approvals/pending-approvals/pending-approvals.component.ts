import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveService } from '../../../core/services/leave.service';
import { LeaveRequestSummary, LeaveStatus } from '../../../core/models/leave.models';

/**
 * Component providing managers with review workflows to approve or reject employee leave requests.
 */
@Component({
  selector: 'app-pending-approvals',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './pending-approvals.component.html',
  styleUrls: ['./pending-approvals.component.scss']
})
export class PendingApprovalsComponent implements OnInit {
  /** Signal containing the collection of leave requests currently awaiting managerial action. */
  public pendingRequests = signal<LeaveRequestSummary[]>([]);
  /** Loading indicator signal for asynchronous fetch states. */
  public isLoading = signal<boolean>(true);
  /** Error message signal for request failures. */
  public errorMessage = signal<string | null>(null);
  /** Success notification signal displayed after completing a review action. */
  public successMessage = signal<string | null>(null);
  /** In-flight indicator to disable action buttons during API processing. */
  public isProcessing = signal<boolean>(false);

  /** Active modal state holding the target request for decision confirmation. */
  public selectedRequest: LeaveRequestSummary | null = null;
  /** Flag tracking whether the active modal is an approval (true) or rejection (false). */
  public isApprovalAction = true;
  /** Reviewer comment bound via two-way template binding. */
  public reviewComment = '';

  /**
   * Initializes the leave service dependency.
   * @param leaveService Service handling leave and review API operations.
   */
  constructor(private leaveService: LeaveService) {}

  /**
   * Angular lifecycle hook executed on initialization to retrieve pending applications.
   */
  public ngOnInit(): void {
    this.loadPendingApprovals();
  }

  /**
   * Fetches all pending leave applications across reporting staff.
   */
  public loadPendingApprovals(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.leaveService.getPendingApprovals().subscribe({
      next: (data) => {
        this.pendingRequests.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.detail || 'Failed to retrieve pending approvals.');
        this.isLoading.set(false);
      }
    });
  }

  /**
   * Opens the confirmation dialog for an approval or rejection decision.
   * @param request The leave application selected for review.
   * @param isApprove True to trigger approval flow, false for rejection flow.
   */
  public openReviewModal(request: LeaveRequestSummary, isApprove: boolean): void {
    this.selectedRequest = request;
    this.isApprovalAction = isApprove;
    this.reviewComment = '';
  }

  /**
   * Closes the active confirmation dialog and clears selection state.
   */
  public closeReviewModal(): void {
    this.selectedRequest = null;
    this.reviewComment = '';
  }

  /**
   * Submits the manager's review decision to the backend API.
   */
  public submitReview(): void {
    if (!this.selectedRequest) return;

    this.isProcessing.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const requestId = this.selectedRequest.id;
    const action = this.isApprovalAction ? LeaveStatus.Approved : LeaveStatus.Rejected;
    const actionName = this.isApprovalAction ? 'approved' : 'rejected';

    this.leaveService
      .reviewLeaveRequest(requestId, action, this.reviewComment)
      .subscribe({
        next: () => {
          this.isProcessing.set(false);
          this.closeReviewModal();
          this.successMessage.set(`Request #${requestId} has been successfully ${actionName}.`);
          this.loadPendingApprovals();
        },
        error: (err) => {
          this.isProcessing.set(false);
          this.errorMessage.set(
            err.error?.detail || err.error?.message || `Failed to complete ${actionName} action.`
          );
        }
      });
  }
}
