import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateLeaveRequest,
  LeaveBalance,
  LeaveRequestSummary,
  LeaveStatus
} from '../models/leave.models';

/**
 * Service managing leave applications, allocation balances, and manager review actions.
 */
@Injectable({
  providedIn: 'root'
})
export class LeaveService {
  private readonly baseUrl = `${environment.apiUrl}`;

  /**
   * Initializes the HTTP transport dependency.
   * @param http Angular HTTP client service.
   */
  constructor(private http: HttpClient) {}

  /**
   * Fetches annual leave balances for the currently authenticated employee.
   * @returns Observable collection of leave balance records.
   */
  public getMyBalances(): Observable<LeaveBalance[]> {
    return this.http.get<LeaveBalance[]>(`${this.baseUrl}/leaves/balances`);
  }

  /**
   * Fetches the historical collection of leave applications for the current employee.
   * @returns Observable collection of leave request summaries.
   */
  public getMyRequests(): Observable<LeaveRequestSummary[]> {
    return this.http.get<LeaveRequestSummary[]>(`${this.baseUrl}/leaves/my-history`);
  }

  /**
   * Submits a new leave booking application.
   * @param payload Target leave details including dates, category, and justification.
   * @returns Observable emitting the newly created leave record summary.
   */
  public submitLeaveRequest(payload: CreateLeaveRequest): Observable<LeaveRequestSummary> {
    return this.http.post<LeaveRequestSummary>(`${this.baseUrl}/leaves/request`, payload);
  }

  /**
   * Fetches all pending leave applications requiring manager review.
   * @returns Observable collection of pending leave applications across reporting staff.
   */
  public getPendingApprovals(): Observable<LeaveRequestSummary[]> {
    return this.http.get<LeaveRequestSummary[]>(`${this.baseUrl}/approvals/pending`);
  }

  /**
   * Submits an approval or rejection decision for a pending leave request.
   * @param leaveId Primary identifier of the leave application.
   * @param decision Decision: approved or rejected.
   * @param comments Optional reviewer notes or rejection justification.
   * @returns Observable representing API completion.
   */
  public reviewLeaveRequest(leaveId: number, decision: LeaveStatus, comments?: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/approvals/${leaveId}/decision`, { decision, comments });
  }
}
