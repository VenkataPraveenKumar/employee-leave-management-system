using EmployeeLeaveManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Approval> Approvals => Set<Approval>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- User Configuration ---
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(50).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

            // Self-referencing relationship: Manager -> Subordinates
            entity.HasOne(u => u.Manager)
                  .WithMany()
                  .HasForeignKey(u => u.ManagerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- LeaveBalance Configuration ---
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.HasKey(lb => lb.Id);
            entity.Property(lb => lb.LeaveType).HasConversion<string>().HasMaxLength(20);

            // Ensure one balance record per user per leave type per year
            entity.HasIndex(lb => new { lb.UserId, lb.LeaveType, lb.Year }).IsUnique();

            entity.HasOne(lb => lb.User)
                  .WithMany(u => u.LeaveBalances)
                  .HasForeignKey(lb => lb.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- LeaveRequest Configuration ---
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.HasKey(lr => lr.Id);
            entity.Property(lr => lr.LeaveType).HasConversion<string>().HasMaxLength(20);
            entity.Property(lr => lr.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(lr => lr.Reason).HasMaxLength(500).IsRequired();

            entity.HasOne(lr => lr.User)
                  .WithMany(u => u.LeaveRequests)
                  .HasForeignKey(lr => lr.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Approval Configuration ---
        modelBuilder.Entity<Approval>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Decision).HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Comments).HasMaxLength(500);

            // 1-to-1 relationship with LeaveRequest
            entity.HasOne(a => a.LeaveRequest)
                  .WithOne(lr => lr.Approval)
                  .HasForeignKey<Approval>(a => a.LeaveRequestId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Approver)
                  .WithMany()
                  .HasForeignKey(a => a.ApproverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
