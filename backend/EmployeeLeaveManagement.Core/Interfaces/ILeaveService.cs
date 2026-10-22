using EmployeeLeaveManagement.Core.DTOs;

namespace EmployeeLeaveManagement.Core.Interfaces;

/// <summary>
/// Defines business operations for employee leave balance tracking and leave request submissions.
/// </summary>
public interface ILeaveService
{
    /// <summary>
    /// Retrieves all allocated, utilized, and remaining leave balances for a specific user for the current year.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of leave balances categorized by leave type.</returns>
    Task<IReadOnlyList<LeaveBalanceDto>> GetUserBalancesAsync(int userId);

    /// <summary>
    /// Retrieves the historical and active leave requests submitted by a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of leave requests ordered chronologically with newest first.</returns>
    Task<IReadOnlyList<LeaveRequestResponseDto>> GetUserLeaveHistoryAsync(int userId);

    /// <summary>
    /// Validates business rules and submits a new leave request for approval.
    /// </summary>
    /// <param name="userId">The unique identifier of the submitting user.</param>
    /// <param name="dto">The leave request payload containing dates, leave type, and reason.</param>
    /// <returns>The created leave request record with a pending status.</returns>
    /// <exception cref="ArgumentException">Thrown when date ranges violate past-date or ordering rules.</exception>
    /// <exception cref="InvalidOperationException">Thrown when overlapping leaves exist or leave balance is insufficient.</exception>
    Task<LeaveRequestResponseDto> SubmitLeaveRequestAsync(int userId, CreateLeaveRequestDto dto);

    /// <summary>
    /// Retrieves all pending leave requests for direct reportees assigned to a specific manager.
    /// </summary>
    /// <param name="managerId">The unique identifier of the manager.</param>
    /// <returns>A collection of pending leave requests with applicant details.</returns>
    Task<IReadOnlyList<ManagerLeaveRequestDto>> GetPendingRequestsForManagerAsync(int managerId);

    /// <summary>
    /// Processes a manager's decision (Approve/Reject) on a leave request and updates the applicant's leave balance accordingly.
    /// </summary>
    /// <param name="leaveRequestId">The unique identifier of the leave request.</param>
    /// <param name="managerId">The unique identifier of the deciding manager.</param>
    /// <param name="dto">The decision payload containing status and optional feedback comments.</param>
    /// <returns>True if operation succeeded.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the leave request or balance record cannot be found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the manager is not authorized to act on this reportee.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the request has already been processed or insufficient balance exists.</exception>
    Task<bool> ProcessLeaveDecisionAsync(int leaveRequestId, int managerId, LeaveDecisionDto dto);
}
