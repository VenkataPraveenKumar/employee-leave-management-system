using EmployeeLeaveManagement.Core.DTOs;
using EmployeeLeaveManagement.Core.Entities;
using EmployeeLeaveManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLeaveManagementSystem.API.Controllers;

/// <summary>
/// Provides authentication endpoints for user login and identity introspection.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="unitOfWork">The Unit of Work coordinator for accessing user records.</param>
    /// <param name="tokenService">The JWT generator service.</param>
    public AuthController(IUnitOfWork unitOfWork, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    /// <summary>
    /// Authenticates user credentials and generates a signed JWT token.
    /// </summary>
    /// <param name="request">The login credentials (email and plain password).</param>
    /// <response code="200">Authentication successful, returns user identity and bearer token.</response>
    /// <response code="401">Invalid credentials provided.</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request)
    {
        var users = await _unitOfWork.Repository<User>().FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _tokenService.GenerateToken(user);
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        var response = new AuthResponseDto(
            user.Id,
            $"{user.FirstName} {user.LastName}",
            user.Email,
            user.Role.ToString(),
            token,
            expiresAt
        );

        return Ok(response);
    }

    /// <summary>
    /// Returns the claims and identity of the currently authenticated bearer token.
    /// </summary>
    /// <response code="200">Returns user ID, email, and role from JWT claims.</response>
    /// <response code="401">Unauthorized if the token is missing or invalid.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

        return Ok(new
        {
            Message = "You are authenticated!",
            UserId = userId,
            Email = email,
            Role = role
        });
    }
}
