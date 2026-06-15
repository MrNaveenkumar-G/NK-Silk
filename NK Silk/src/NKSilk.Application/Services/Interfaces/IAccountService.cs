using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IAccountService
{
    /// <summary>Creates a customer account. Fails if the email is already registered.</summary>
    Task<AuthResult> RegisterAsync(RegisterVm vm, CancellationToken ct = default);

    /// <summary>Verifies email + password. Returns a failed result for any mismatch.</summary>
    Task<AuthResult> LoginAsync(LoginVm vm, CancellationToken ct = default);

    /// <summary>Confirms an email-verification token; returns true when a match is verified.</summary>
    Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default);

    /// <summary>
    /// Issues a password-reset token for the email and dispatches a reset link.
    /// Always succeeds silently (never reveals whether the email exists).
    /// </summary>
    Task RequestPasswordResetAsync(string email, string resetUrlTemplate, CancellationToken ct = default);

    /// <summary>Resets the password if the token is valid and unexpired.</summary>
    Task<AuthResult> ResetPasswordAsync(ResetPasswordVm vm, CancellationToken ct = default);
}
