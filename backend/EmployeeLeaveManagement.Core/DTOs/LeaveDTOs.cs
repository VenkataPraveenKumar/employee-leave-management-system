using EmployeeLeaveManagement.Core.Enums;

namespace EmployeeLeaveManagement.Core.DTOs;

public record LeaveBalanceDto(
    int Id,
    string LeaveType,
    int TotalDays,
    int UsedDays,
    int RemainingDays,
    int Year
);

public record CreateLeaveRequestDto(
    LeaveType LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    string Reason
);

public record LeaveRequestResponseDto(
    int Id,
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    string Reason,
    string Status,
    DateTime CreatedAtUtc
);

/// <summary>
/// Payload submitted by a manager to decide on a pending leave request.
/// </summary>
public record LeaveDecisionDto(
    LeaveStatus Decision,
    string? Comments
);

/// <summary>
/// Detailed response model of a leave request visible to managers, including applicant info.
/// </summary>
public record ManagerLeaveRequestDto(
    int Id,
    int EmployeeId,
    string EmployeeName,
    string EmployeeEmail,
    string LeaveType,
    DateOnly StartDate,
    DateOnly EndDate,
    int TotalDays,
    string Reason,
    string Status,
    DateTime CreatedAtUtc
);
