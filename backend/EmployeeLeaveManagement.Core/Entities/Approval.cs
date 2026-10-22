using EmployeeLeaveManagement.Core.Enums;

namespace EmployeeLeaveManagement.Core.Entities;

public class Approval
{
    public int Id { get; set; }
    public int LeaveRequestId { get; set; }
    public LeaveRequest LeaveRequest { get; set; } = null!;
    public int ApproverId { get; set; }
    public User Approver { get; set; } = null!;
    public LeaveStatus Decision { get; set; }
    public string? Comments { get; set; }
    public DateTime DecidedAtUtc { get; set; } = DateTime.UtcNow;
}
