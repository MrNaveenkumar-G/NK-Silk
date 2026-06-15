using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Web.Infrastructure;
using NKSilk.Web.Models;

namespace NKSilk.Web.Controllers.Api;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthApiController : ControllerBase
{
    private readonly IAccountService _accounts;
    private readonly JwtTokenService _jwt;

    public AuthApiController(IAccountService accounts, JwtTokenService jwt)
    {
        _accounts = accounts;
        _jwt = jwt;
    }

    public record AuthResponse(string Token, DateTime ExpiresAtUtc, int CustomerId, string FullName, string[] Roles);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<AuthResponse>.Fail("validation", "Invalid registration details."));
        var result = await _accounts.RegisterAsync(vm, ct);
        return result.Succeeded ? Ok(Issue(result)) : BadRequest(ApiResponse<AuthResponse>.Fail("register_failed", result.Error!));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ApiResponse<AuthResponse>.Fail("validation", "Email and password are required."));
        var result = await _accounts.LoginAsync(vm, ct);
        return result.Succeeded
            ? Ok(Issue(result))
            : Unauthorized(ApiResponse<AuthResponse>.Fail("invalid_credentials", result.Error!));
    }

    private ApiResponse<AuthResponse> Issue(AuthResult result)
    {
        var roles = new HashSet<string>(result.Roles, StringComparer.OrdinalIgnoreCase);
        if (result.IsAdmin) roles.Add("Admin");
        if (result.IsVendor) roles.Add("Vendor");
        var (token, expires) = _jwt.Generate(result.CustomerId, result.FullName, result.Email, roles);
        return ApiResponse<AuthResponse>.Ok(new AuthResponse(token, expires, result.CustomerId, result.FullName, roles.ToArray()));
    }
}
