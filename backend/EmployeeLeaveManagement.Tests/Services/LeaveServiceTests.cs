using System.Linq.Expressions;
using EmployeeLeaveManagement.Core.DTOs;
using EmployeeLeaveManagement.Core.Entities;
using EmployeeLeaveManagement.Core.Enums;
using EmployeeLeaveManagement.Core.Interfaces;
using EmployeeLeaveManagement.Infrastructure.Services;
using Moq;
using Xunit;

namespace EmployeeLeaveManagement.Tests.Services;

/// <summary>
/// Unit tests validating domain rules and constraints within the <see cref="LeaveService"/>.
/// </summary>
public class LeaveServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IGenericRepository<LeaveRequest>> _mockLeaveRequestRepo;
    private readonly Mock<IGenericRepository<LeaveBalance>> _mockLeaveBalanceRepo;
    private readonly Mock<IGenericRepository<User>> _mockUserRepo;
    private readonly Mock<IGenericRepository<Approval>> _mockApprovalRepo;
    private readonly LeaveService _sut; // System Under Test

    /// <summary>
    /// Initializes test dependencies and configures repository mocks.
    /// </summary>
    public LeaveServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLeaveRequestRepo = new Mock<IGenericRepository<LeaveRequest>>();
        _mockLeaveBalanceRepo = new Mock<IGenericRepository<LeaveBalance>>();
        _mockUserRepo = new Mock<IGenericRepository<User>>();
        _mockApprovalRepo = new Mock<IGenericRepository<Approval>>();

        // Bind mock repositories to Unit of Work factory
        _mockUnitOfWork.Setup(u => u.Repository<LeaveRequest>()).Returns(_mockLeaveRequestRepo.Object);
        _mockUnitOfWork.Setup(u => u.Repository<LeaveBalance>()).Returns(_mockLeaveBalanceRepo.Object);
        _mockUnitOfWork.Setup(u => u.Repository<User>()).Returns(_mockUserRepo.Object);
        _mockUnitOfWork.Setup(u => u.Repository<Approval>()).Returns(_mockApprovalRepo.Object);

        _sut = new LeaveService(_mockUnitOfWork.Object);
    }

    /// <summary>
    /// Verifies that non-sick leave submissions for past dates throw an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public async Task SubmitLeaveRequestAsync_PastDateForPaidLeave_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
        var dto = new CreateLeaveRequestDto(
            LeaveType: LeaveType.Paid,
            StartDate: pastDate,
            EndDate: pastDate,
            Reason: "Vacation"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.SubmitLeaveRequestAsync(userId, dto));

        Assert.Contains("Paid leave cannot be applied for past dates", exception.Message);
    }

    /// <summary>
    /// Verifies that sick leave beyond the 5-day grace window throws an <see cref="ArgumentException"/>.
    /// </summary>
    [Fact]
    public async Task SubmitLeaveRequestAsync_SickLeaveExceedingGracePeriod_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var pastDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-6));
        var dto = new CreateLeaveRequestDto(
            LeaveType: LeaveType.Sick,
            StartDate: pastDate,
            EndDate: pastDate,
            Reason: "Old fever"
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _sut.SubmitLeaveRequestAsync(userId, dto));

        Assert.Contains("Sick leave cannot be applied more than 5 days in the past", exception.Message);
    }

    /// <summary>
    /// Verifies that overlapping leave applications throw an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task SubmitLeaveRequestAsync_OverlappingExistingLeave_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5));
        var dto = new CreateLeaveRequestDto(LeaveType.Paid, startDate, endDate, "Holiday");

        var existingRequests = new List<LeaveRequest>
        {
            new()
            {
                Id = 10,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Status = LeaveStatus.Approved
            }
        };

        _mockLeaveRequestRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(existingRequests);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SubmitLeaveRequestAsync(userId, dto));

        Assert.Contains("overlapping", exception.Message);
    }

    /// <summary>
    /// Verifies that attempting to apply for more days than available balance throws an <see cref="InvalidOperationException"/>.
    /// </summary>
    [Fact]
    public async Task SubmitLeaveRequestAsync_InsufficientBalance_ThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(15)); // 6 days requested
        var dto = new CreateLeaveRequestDto(LeaveType.Casual, startDate, endDate, "Trip");

        _mockLeaveRequestRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(new List<LeaveRequest>()); // No overlaps

        var lowBalances = new List<LeaveBalance>
        {
            new()
            {
                UserId = userId,
                LeaveType = LeaveType.Casual,
                TotalDays = 5,
                UsedDays = 3, // Remaining = 2 days (less than 6 requested)
                Year = startDate.Year
            }
        };

        _mockLeaveBalanceRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<LeaveBalance, bool>>>()))
            .ReturnsAsync(lowBalances);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SubmitLeaveRequestAsync(userId, dto));

        Assert.Contains("Insufficient Casual leave balance", exception.Message);
    }

    /// <summary>
    /// Verifies that valid leave applications are saved and returned with Pending status.
    /// </summary>
    [Fact]
    public async Task SubmitLeaveRequestAsync_ValidRequest_CreatesPendingLeaveSuccessfully()
    {
        // Arrange
        var userId = 1;
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(12)); // 3 days requested
        var dto = new CreateLeaveRequestDto(LeaveType.Paid, startDate, endDate, "Family Event");

        _mockLeaveRequestRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<LeaveRequest, bool>>>()))
            .ReturnsAsync(new List<LeaveRequest>()); // No overlaps

        var sufficientBalances = new List<LeaveBalance>
        {
            new()
            {
                UserId = userId,
                LeaveType = LeaveType.Paid,
                TotalDays = 18,
                UsedDays = 2, // Remaining = 16 (sufficient)
                Year = startDate.Year
            }
        };

        _mockLeaveBalanceRepo
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<LeaveBalance, bool>>>()))
            .ReturnsAsync(sufficientBalances);

        _mockLeaveRequestRepo
            .Setup(r => r.AddAsync(It.IsAny<LeaveRequest>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.CompleteAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _sut.SubmitLeaveRequestAsync(userId, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(3, result.TotalDays);
        _mockLeaveRequestRepo.Verify(r => r.AddAsync(It.IsAny<LeaveRequest>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// Verifies that manager approval atomically deducts days from the employee balance and marks status Approved.
    /// </summary>
    [Fact]
    public async Task ProcessLeaveDecisionAsync_Approved_DeductsBalanceAndCompletes()
    {
        // Arrange
        var leaveRequestId = 5;
        var managerId = 2;
        var employeeId = 3;

        var existingRequest = new LeaveRequest
        {
            Id = leaveRequestId,
            UserId = employeeId,
            LeaveType = LeaveType.Paid,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            TotalDays = 3,
            Status = LeaveStatus.Pending
        };

        var employeeUser = new User
        {
            Id = employeeId,
            ManagerId = managerId // Confirms manager relationship
        };

        var balance = new LeaveBalance
        {
            UserId = employeeId,
            LeaveType = LeaveType.Paid,
            TotalDays = 18,
            UsedDays = 2,
            Year = existingRequest.StartDate.Year
        };

        _mockLeaveRequestRepo.Setup(r => r.GetByIdAsync(leaveRequestId)).ReturnsAsync(existingRequest);
        _mockUserRepo.Setup(u => u.GetByIdAsync(employeeId)).ReturnsAsync(employeeUser);
        _mockLeaveBalanceRepo
            .Setup(b => b.FindAsync(It.IsAny<Expression<Func<LeaveBalance, bool>>>()))
            .ReturnsAsync(new List<LeaveBalance> { balance });

        var decisionDto = new LeaveDecisionDto(LeaveStatus.Approved, "Approved by manager");

        // Act
        var result = await _sut.ProcessLeaveDecisionAsync(leaveRequestId, managerId, decisionDto);

        // Assert
        Assert.True(result);
        Assert.Equal(LeaveStatus.Approved, existingRequest.Status);
        Assert.Equal(5, balance.UsedDays); // 2 previous + 3 deducted = 5
        _mockApprovalRepo.Verify(a => a.AddAsync(It.IsAny<Approval>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.CompleteAsync(), Times.Once);
    }
}
