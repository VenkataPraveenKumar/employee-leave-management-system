using EmployeeLeaveManagement.Core.Entities;
using EmployeeLeaveManagement.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Ensure database is created/migrated
        await context.Database.MigrateAsync();

        // 1. Seed Users if none exist
        if (!await context.Users.AnyAsync())
        {
            var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword("Password@123");

            var admin = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = "admin@leaveapp.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Admin,
                CreatedAtUtc = DateTime.UtcNow
            };

            var manager = new User
            {
                FirstName = "Sarah",
                LastName = "Connor",
                Email = "manager@leaveapp.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Manager,
                CreatedAtUtc = DateTime.UtcNow
            };

            context.Users.AddRange(admin, manager);
            await context.SaveChangesAsync();

            var employee = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "employee@leaveapp.com",
                PasswordHash = defaultPasswordHash,
                Role = Role.Employee,
                ManagerId = manager.Id,
                CreatedAtUtc = DateTime.UtcNow
            };

            context.Users.Add(employee);
            await context.SaveChangesAsync();

            // 2. Seed Default Leave Balances for the Employee
            var currentYear = DateTime.UtcNow.Year;
            var balances = new List<LeaveBalance>
            {
                new() { UserId = employee.Id, LeaveType = LeaveType.Paid, TotalDays = 18, UsedDays = 0, Year = currentYear },
                new() { UserId = employee.Id, LeaveType = LeaveType.Sick, TotalDays = 12, UsedDays = 0, Year = currentYear },
                new() { UserId = employee.Id, LeaveType = LeaveType.Casual, TotalDays = 6, UsedDays = 0, Year = currentYear }
            };

            context.LeaveBalances.AddRange(balances);
            await context.SaveChangesAsync();
        }
    }
}
