import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { LeaveService } from '../../core/services/leave.service';
import { LeaveBalance, LeaveRequestSummary } from '../../core/models/leave.models';

/**
 * Dashboard view providing real-time leave entitlement metrics, recent booking activity, and direct action shortcuts.
 */
@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  /** Signal containing annual leave balances. */
  public balances = signal<LeaveBalance[]>([]);
  /** Signal containing recent leave requests. */
  public recentRequests = signal<LeaveRequestSummary[]>([]);
  /** Loading indicator signal for asynchronous fetch states. */
  public isLoading = signal<boolean>(true);
  /** Error message signal for request failures. */
  public errorMessage = signal<string | null>(null);

  /**
   * Initializes required services for identity and leave queries.
   * @param authService Service exposing current identity signals.
   * @param leaveService Service handling leave API communication.
   */
  constructor(
    public authService: AuthService,
    private leaveService: LeaveService
  ) {}

  /**
   * Angular lifecycle hook executed on component mount to retrieve dashboard metrics.
   */
  public ngOnInit(): void {
    this.loadDashboardData();
  }

  /**
   * Fetches both leave balances and recent booking history concurrently.
   */
  public loadDashboardData(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.leaveService.getMyBalances().subscribe({
      next: (data) => {
        this.balances.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(err.error?.detail || 'Unable to load leave balances.');
        this.isLoading.set(false);
      }
    });

    this.leaveService.getMyRequests().subscribe({
      next: (data) => {
        this.recentRequests.set(data.slice(0, 5));
      },
      error: () => {
        // Balances error handles user feedback
      }
    });
  }
}
