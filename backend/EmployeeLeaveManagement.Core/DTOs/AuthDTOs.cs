namespace EmployeeLeaveManagement.Core.DTOs;

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(
    int Id,
    string FullName,
    string Email,
    string Role,
    string Token,
    DateTime ExpiresAtUtc
);
