using System.Security.Claims;
using EmployeeLeaveManagement.Core.DTOs;
using EmployeeLeaveManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagementSystem.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for authenticated employees to manage their leaves and balances.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LeavesController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeavesController"/> class.
    /// </summary>
    /// <param name="leaveService">The leave service domain handler.</param>
    public LeavesController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    /// <summary>
    /// Extracts and parses the authenticated user's ID from the JWT claims context.
    /// </summary>
    /// <returns>The integer user ID.</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown if the claim is missing or malformed.</exception>
    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var userId))
        {
            throw new UnauthorizedAccessException("User context is missing or invalid.");
        }
        return userId;
    }

    /// <summary>
    /// Retrieves current annual leave balances (Total, Used, Remaining) for the authenticated employee.
    /// </summary>
    /// <response code="200">Returns the employee's leave balance details.</response>
    /// <response code="401">Unauthorized if the JWT is missing or expired.</response>
    [HttpGet("balances")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<LeaveBalanceDto>>> GetMyBalances()
    {
        var userId = GetCurrentUserId();
        var balances = await _leaveService.GetUserBalancesAsync(userId);
        return Ok(balances);
    }

    /// <summary>
    /// Retrieves historical and pending leave applications submitted by the authenticated employee.
    /// </summary>
    /// <response code="200">Returns list of previous leave requests.</response>
    /// <response code="401">Unauthorized if the JWT is missing or expired.</response>
    [HttpGet("my-history")]
    [ProducesResponseType(typeof(IReadOnlyList<LeaveRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestResponseDto>>> GetMyLeaveHistory()
    {
        var userId = GetCurrentUserId();
        var history = await _leaveService.GetUserLeaveHistoryAsync(userId);
        return Ok(history);
    }

    /// <summary>
    /// Submits a new leave request subject to balance availability and business validation rules.
    /// </summary>
    /// <param name="dto">The leave application payload.</param>
    /// <response code="200">Leave request submitted successfully in Pending state.</response>
    /// <response code="400">Invalid dates or past-date rules violated.</response>
    /// <response code="409">Overlapping leave or insufficient balance.</response>
    [HttpPost("request")]
    [ProducesResponseType(typeof(LeaveRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LeaveRequestResponseDto>> SubmitLeaveRequest([FromBody] CreateLeaveRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var result = await _leaveService.SubmitLeaveRequestAsync(userId, dto);
        return Ok(result);
    }
}
