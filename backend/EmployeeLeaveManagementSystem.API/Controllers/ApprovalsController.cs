using System.Security.Claims;
using EmployeeLeaveManagement.Core.DTOs;
using EmployeeLeaveManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagementSystem.API.Controllers;

/// <summary>
/// Provides administrative and manager operations to review and process leave applications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Manager,Admin")]
public class ApprovalsController : ControllerBase
{
    private readonly ILeaveService _leaveService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApprovalsController"/> class.
    /// </summary>
    /// <param name="leaveService">The leave service domain handler.</param>
    public ApprovalsController(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

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
    /// Retrieves all pending leave requests submitted by direct subordinates of the logged-in manager.
    /// </summary>
    /// <response code="200">Returns list of pending leave requests.</response>
    /// <response code="403">Forbidden if the caller does not have the Manager or Admin role.</response>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(IReadOnlyList<ManagerLeaveRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<ManagerLeaveRequestDto>>> GetPendingRequests()
    {
        var managerId = GetCurrentUserId();
        var requests = await _leaveService.GetPendingRequestsForManagerAsync(managerId);
        return Ok(requests);
    }

    /// <summary>
    /// Records an approval or rejection decision on a pending leave application.
    /// </summary>
    /// <param name="id">The leave request ID.</param>
    /// <param name="dto">The decision payload (Approved = 2, Rejected = 3) with comments.</param>
    /// <response code="200">The decision was processed successfully.</response>
    /// <response code="400">Invalid decision value provided.</response>
    /// <response code="403">Caller is not authorized to act on this reportee.</response>
    /// <response code="404">Leave request not found.</response>
    /// <response code="409">Request has already been processed or applicant has insufficient balance.</response>
    [HttpPost("{id:int}/decision")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ProcessDecision(int id, [FromBody] LeaveDecisionDto dto)
    {
        var managerId = GetCurrentUserId();
        await _leaveService.ProcessLeaveDecisionAsync(id, managerId, dto);
        return Ok(new { message = $"Leave request #{id} successfully {dto.Decision.ToString().ToLower()}." });
    }
}
