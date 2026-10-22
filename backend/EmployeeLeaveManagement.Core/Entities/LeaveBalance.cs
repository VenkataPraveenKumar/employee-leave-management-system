using EmployeeLeaveManagement.Core.Enums;

namespace EmployeeLeaveManagement.Core.Entities;

public class LeaveBalance
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public LeaveType LeaveType { get; set; }
    public int TotalDays { get; set; }
    public int UsedDays { get; set; }
    public int RemainingDays => TotalDays - UsedDays;
    public int Year { get; set; }
}
