/**
 * Supported leave categories matching the backend domain enums.
 */
export enum LeaveType {
  Sick = 1,
  Casual = 2,
  Paid = 3,
  Unpaid = 4
}

/**
 * Workflow status states of a leave request application.
 */
export enum LeaveStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3,
  Cancelled = 4
}

/**
 * Representation of an employee's annual leave balance for a calendar year.
 */
export interface LeaveBalance {
  /** Unique identifier for the leave balance record. */
  id: number;
  /** Category of leave (e.g., 'Paid', 'Sick', 'Casual'). */
  leaveType: string;
  /** Total days credited for the current year. */
  totalDays: number;
  /** Total days approved or consumed. */
  usedDays: number;
  /** Total days remaining available for leave requests. */
  remainingDays: number;
  /** Calendar year associated with this allocation. */
  year: number;
}

/**
 * Payload sent by an employee to request new leave.
 */
export interface CreateLeaveRequest {
  /** The selected leave category enum value. */
  leaveType: LeaveType;
  /** The starting date in ISO string format (YYYY-MM-DD). */
  startDate: string;
  /** The ending date in ISO string format (YYYY-MM-DD). */
  endDate: string;
  /** Detailed reason or justification for taking leave. */
  reason: string;
}

/**
 * Representation of a submitted leave request displayed in user history.
 */
export interface LeaveRequestSummary {
  /** Unique identifier of the leave request. */
  id: number;
  /** The leave category name. */
  leaveType: string;
  /** Starting date of the leave period. */
  startDate: string;
  /** Ending date of the leave period. */
  endDate: string;
  /** Total working days consumed by the period. */
  totalDays: number;
  /** Justification text provided by applicant. */
  reason: string;
  /** Current workflow status ('Pending', 'Approved', 'Rejected'). */
  status: string;
  /** UTC timestamp when the request was submitted. */
  createdAtUtc: string;
}
