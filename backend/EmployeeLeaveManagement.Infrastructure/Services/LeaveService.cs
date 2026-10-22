using EmployeeLeaveManagement.Core.DTOs;
using EmployeeLeaveManagement.Core.Entities;
using EmployeeLeaveManagement.Core.Enums;
using EmployeeLeaveManagement.Core.Interfaces;

namespace EmployeeLeaveManagement.Infrastructure.Services;

/// <summary>
/// Implements business logic and data access workflows for employee leave management.
/// </summary>
public class LeaveService : ILeaveService
{
    private readonly IUnitOfWork _unitOfWork;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaveService"/> class.
    /// </summary>
    /// <param name="unitOfWork">The Unit of Work coordinator for database transactions.</param>
    public LeaveService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Fetches all active leave balances for an employee for the current calendar year.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A read-only list of <see cref="LeaveBalanceDto"/> records.</returns>
    public async Task<IReadOnlyList<LeaveBalanceDto>> GetUserBalancesAsync(int userId)
    {
        var currentYear = DateTime.UtcNow.Year;
        var balances = await _unitOfWork.Repository<LeaveBalance>()
            .FindAsync(b => b.UserId == userId && b.Year == currentYear);

        return balances.Select(b => new LeaveBalanceDto(
            b.Id,
            b.LeaveType.ToString(),
            b.TotalDays,
            b.UsedDays,
            b.RemainingDays,
            b.Year
        )).ToList();
    }

    /// <summary>
    /// Retrieves all historical leave applications submitted by the user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A list of past and pending leave requests sorted by submission date descending.</returns>
    public async Task<IReadOnlyList<LeaveRequestResponseDto>> GetUserLeaveHistoryAsync(int userId)
    {
        var requests = await _unitOfWork.Repository<LeaveRequest>()
            .FindAsync(r => r.UserId == userId);

        return requests
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new LeaveRequestResponseDto(
                r.Id,
                r.LeaveType.ToString(),
                r.StartDate,
                r.EndDate,
                r.TotalDays,
                r.Reason,
                r.Status.ToString(),
                r.CreatedAtUtc
            )).ToList();
    }

    /// <summary>
    /// Evaluates HR validation rules (retrospective sick leave grace period, date sequence,
    /// calendar overlap checks, and remaining balance verification) before persisting a leave application.
    /// </summary>
    /// <param name="userId">The unique identifier of the applicant.</param>
    /// <param name="dto">The leave details submitted by the applicant.</param>
    /// <returns>The newly created <see cref="LeaveRequestResponseDto"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when dates are invalid or exceed allowable retrospective windows.</exception>
    /// <exception cref="InvalidOperationException">Thrown when dates overlap with existing leaves or available balance is exceeded.</exception>
    public async Task<LeaveRequestResponseDto> SubmitLeaveRequestAsync(int userId, CreateLeaveRequestDto dto)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Rule 1: Date Range & Retrospective Sick Leave Grace Period
        const int maxPastSickDays = 5;

        if (dto.LeaveType == LeaveType.Sick)
        {
            var earliestAllowedDate = today.AddDays(-maxPastSickDays);
            if (dto.StartDate < earliestAllowedDate)
            {
                throw new ArgumentException($"Sick leave cannot be applied more than {maxPastSickDays} days in the past.");
            }
        }
        else
        {
            if (dto.StartDate < today)
            {
                throw new ArgumentException($"{dto.LeaveType} leave cannot be applied for past dates.");
            }
        }

        if (dto.EndDate < dto.StartDate)
        {
            throw new ArgumentException("Leave end date must be on or after start date.");
        }

        var totalDays = (dto.EndDate.DayNumber - dto.StartDate.DayNumber) + 1;

        // Rule 2: Overlapping Leaves Check (Pending or Approved)
        var existingRequests = await _unitOfWork.Repository<LeaveRequest>()
            .FindAsync(r => r.UserId == userId &&
                            (r.Status == LeaveStatus.Pending || r.Status == LeaveStatus.Approved) &&
                            r.StartDate <= dto.EndDate &&
                            r.EndDate >= dto.StartDate);

        if (existingRequests.Any())
        {
            throw new InvalidOperationException("You already have a pending or approved leave overlapping with these dates.");
        }

        // Rule 3: Remaining Balance Verification
        var currentYear = dto.StartDate.Year;
        var balances = await _unitOfWork.Repository<LeaveBalance>()
            .FindAsync(b => b.UserId == userId && b.LeaveType == dto.LeaveType && b.Year == currentYear);

        var balance = balances.FirstOrDefault();

        if (balance == null || balance.RemainingDays < totalDays)
        {
            throw new InvalidOperationException($"Insufficient {dto.LeaveType} leave balance. Remaining: {balance?.RemainingDays ?? 0} day(s), Requested: {totalDays} day(s).");
        }

        // Create and persist the leave application
        var leaveRequest = new LeaveRequest
        {
            UserId = userId,
            LeaveType = dto.LeaveType,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalDays = totalDays,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Repository<LeaveRequest>().AddAsync(leaveRequest);
        await _unitOfWork.CompleteAsync();

        return new LeaveRequestResponseDto(
            leaveRequest.Id,
            leaveRequest.LeaveType.ToString(),
            leaveRequest.StartDate,
            leaveRequest.EndDate,
            leaveRequest.TotalDays,
            leaveRequest.Reason,
            leaveRequest.Status.ToString(),
            leaveRequest.CreatedAtUtc
        );
    }

    /// <summary>
    /// Retrieves all pending leave requests submitted by direct reportees of the specified manager.
    /// </summary>
    public async Task<IReadOnlyList<ManagerLeaveRequestDto>> GetPendingRequestsForManagerAsync(int managerId)
    {
        // 1. Fetch direct reportees assigned to this manager
        var reportees = await _unitOfWork.Repository<User>()
            .FindAsync(u => u.ManagerId == managerId);

        var reporteeIds = reportees.Select(u => u.Id).ToHashSet();
        if (!reporteeIds.Any())
        {
            return Array.Empty<ManagerLeaveRequestDto>();
        }

        // 2. Fetch pending requests for these reportees
        var pendingRequests = await _unitOfWork.Repository<LeaveRequest>()
            .FindAsync(r => reporteeIds.Contains(r.UserId) && r.Status == LeaveStatus.Pending);

        var reporteeMap = reportees.ToDictionary(u => u.Id);

        return pendingRequests
            .OrderBy(r => r.CreatedAtUtc)
            .Select(r =>
            {
                var employee = reporteeMap[r.UserId];
                return new ManagerLeaveRequestDto(
                    r.Id,
                    r.UserId,
                    $"{employee.FirstName} {employee.LastName}",
                    employee.Email,
                    r.LeaveType.ToString(),
                    r.StartDate,
                    r.EndDate,
                    r.TotalDays,
                    r.Reason,
                    r.Status.ToString(),
                    r.CreatedAtUtc
                );
            }).ToList();
    }

    /// <summary>
    /// Processes approval or rejection. If approved, deducts days from the employee's LeaveBalance atomically.
    /// </summary>
    public async Task<bool> ProcessLeaveDecisionAsync(int leaveRequestId, int managerId, LeaveDecisionDto dto)
    {
        if (dto.Decision != LeaveStatus.Approved && dto.Decision != LeaveStatus.Rejected)
        {
            throw new ArgumentException("Decision must be either Approved or Rejected.");
        }

        // 1. Fetch the leave request
        var leaveRequest = await _unitOfWork.Repository<LeaveRequest>().GetByIdAsync(leaveRequestId);
        if (leaveRequest == null)
        {
            throw new KeyNotFoundException($"Leave request with ID {leaveRequestId} not found.");
        }

        if (leaveRequest.Status != LeaveStatus.Pending)
        {
            throw new InvalidOperationException("This leave request has already been processed.");
        }

        // 2. Verify that this manager is indeed the direct manager of the employee
        var employee = await _unitOfWork.Repository<User>().GetByIdAsync(leaveRequest.UserId);
        if (employee == null || employee.ManagerId != managerId)
        {
            throw new UnauthorizedAccessException("You are not authorized to approve leave requests for this employee.");
        }

        // 3. If Approved, deduct days from the employee's balance
        if (dto.Decision == LeaveStatus.Approved)
        {
            var year = leaveRequest.StartDate.Year;
            var balances = await _unitOfWork.Repository<LeaveBalance>()
                .FindAsync(b => b.UserId == leaveRequest.UserId &&
                                b.LeaveType == leaveRequest.LeaveType &&
                                b.Year == year);

            var balance = balances.FirstOrDefault();
            if (balance == null || balance.RemainingDays < leaveRequest.TotalDays)
            {
                throw new InvalidOperationException("Cannot approve: insufficient remaining leave balance.");
            }

            // Deduct days
            balance.UsedDays += leaveRequest.TotalDays;
            _unitOfWork.Repository<LeaveBalance>().Update(balance);
        }

        // 4. Update the LeaveRequest status
        leaveRequest.Status = dto.Decision;
        _unitOfWork.Repository<LeaveRequest>().Update(leaveRequest);

        // 5. Create the audit Approval record
        var approval = new Approval
        {
            LeaveRequestId = leaveRequest.Id,
            ApproverId = managerId,
            Decision = dto.Decision,
            Comments = dto.Comments,
            DecidedAtUtc = DateTime.UtcNow
        };

        await _unitOfWork.Repository<Approval>().AddAsync(approval);

        // 6. Commit transaction
        await _unitOfWork.CompleteAsync();
        return true;
    }
}
