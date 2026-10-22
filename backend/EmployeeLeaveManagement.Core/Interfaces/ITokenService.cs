using EmployeeLeaveManagement.Core.Entities;

namespace EmployeeLeaveManagement.Core.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
