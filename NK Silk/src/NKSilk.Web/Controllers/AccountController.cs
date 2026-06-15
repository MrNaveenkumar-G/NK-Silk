using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;

namespace NKSilk.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accounts;

    public AccountController(IAccountService accounts) => _accounts = accounts;

    // ---------- Register ----------
    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToLocal(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View(new RegisterVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        var result = await _accounts.RegisterAsync(vm, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        await SignInAsync(result, isPersistent: true);
        return RedirectToLocal(returnUrl);
    }

    // ---------- Login ----------
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToLocal(returnUrl);
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null, CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(vm);

        var result = await _accounts.LoginAsync(vm, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }

        await SignInAsync(result, vm.RememberMe);
        return RedirectToLocal(returnUrl);
    }

    // ---------- Logout ----------
    [HttpPost, ValidateAntiForgeryToken, Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet, Authorize]
    public IActionResult Profile() => View();

    // ---------- Email verification ----------
    [HttpGet]
    public async Task<IActionResult> VerifyEmail(string token, CancellationToken ct)
    {
        ViewData["Verified"] = await _accounts.VerifyEmailAsync(token, ct);
        return View();
    }

    // ---------- Forgot / reset password ----------
    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        var template = $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?token={{token}}";
        await _accounts.RequestPasswordResetAsync(vm.Email, template, ct);
        TempData["Info"] = "If that email is registered, we've sent a password-reset link.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult ResetPassword(string token) => View(new ResetPasswordVm { Token = token });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(vm);
        var result = await _accounts.ResetPasswordAsync(vm, ct);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(vm);
        }
        await SignInAsync(result, isPersistent: false);
        TempData["Info"] = "Your password has been reset.";
        return RedirectToAction("Index", "Home");
    }

    // ---------- helpers ----------
    private Task SignInAsync(AuthResult result, bool isPersistent)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.CustomerId.ToString()),
            new(ClaimTypes.Name, result.FullName),
            new(ClaimTypes.Email, result.Email)
        };
        // Role claims come from the RBAC tables, unioned with the convenience flags as a fallback.
        var roles = new HashSet<string>(result.Roles, StringComparer.OrdinalIgnoreCase);
        if (result.IsAdmin) roles.Add("Admin");
        if (result.IsVendor) roles.Add("Vendor");
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));
        if (result.IsVendor && result.VendorId is int vid)
            claims.Add(new Claim("VendorId", vid.ToString()));
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var props = new AuthenticationProperties
        {
            IsPersistent = isPersistent,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        };
        return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity), props);
    }

    private IActionResult RedirectToLocal(string? returnUrl)
        => Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction("Index", "Home");
}
