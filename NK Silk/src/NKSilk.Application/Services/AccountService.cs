using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

/// <summary>Customer registration &amp; credential validation on the Customer entity.</summary>
public class AccountService : IAccountService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher<Customer> _hasher;
    private readonly INotificationSender _sender;

    public AccountService(IUnitOfWork uow, IPasswordHasher<Customer> hasher, INotificationSender sender)
    {
        _uow = uow;
        _hasher = hasher;
        _sender = sender;
    }

    public async Task<AuthResult> RegisterAsync(RegisterVm vm, CancellationToken ct = default)
    {
        var email = vm.Email.Trim().ToLowerInvariant();
        var repo = _uow.Repository<Customer>();

        var exists = await repo.FirstOrDefaultAsync(c => c.Email == email, ct);
        if (exists is not null)
            return AuthResult.Fail("An account with this email already exists.");

        var customer = new Customer
        {
            FullName = vm.FullName.Trim(),
            Email = email,
            PhoneNumber = string.IsNullOrWhiteSpace(vm.PhoneNumber) ? null : vm.PhoneNumber.Trim(),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
        customer.PasswordHash = _hasher.HashPassword(customer, vm.Password);
        customer.EmailVerificationToken = Guid.NewGuid().ToString("N");

        await repo.AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);

        try
        {
            await _sender.SendEmailAsync(customer.Email, "Verify your NK Silk email",
                $"Welcome to NK Silk! Verify your email here: /Account/VerifyEmail?token={customer.EmailVerificationToken}", ct);
        }
        catch { /* verification email is best-effort */ }

        return AuthResult.Success(customer.Id, customer.FullName, customer.Email, customer.IsAdmin, customer.IsVendor, customer.VendorId);
    }

    public async Task<bool> VerifyEmailAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token)) return false;
        var repo = _uow.Repository<Customer>();
        var customer = await repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(c => c.EmailVerificationToken == token, ct);
        if (customer is null) return false;

        customer.IsEmailVerified = true;
        customer.EmailVerificationToken = null;
        repo.Update(customer);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    public async Task RequestPasswordResetAsync(string email, string resetUrlTemplate, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var repo = _uow.Repository<Customer>();
        var customer = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(c => c.Email == normalized, ct);
        if (customer is null) return; // silent — don't reveal whether the email exists

        customer.PasswordResetToken = Guid.NewGuid().ToString("N");
        customer.PasswordResetExpiresUtc = DateTime.UtcNow.AddHours(2);
        repo.Update(customer);
        await _uow.SaveChangesAsync(ct);

        try
        {
            var link = resetUrlTemplate.Replace("{token}", customer.PasswordResetToken);
            await _sender.SendEmailAsync(customer.Email, "Reset your NK Silk password",
                $"Reset your password (valid 2 hours): {link}", ct);
        }
        catch { /* best-effort */ }
    }

    public async Task<AuthResult> ResetPasswordAsync(ResetPasswordVm vm, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Customer>();
        var customer = await repo.Query(asNoTracking: false)
            .FirstOrDefaultAsync(c => c.PasswordResetToken == vm.Token, ct);
        if (customer is null || customer.PasswordResetExpiresUtc < DateTime.UtcNow)
            return AuthResult.Fail("This reset link is invalid or has expired.");

        customer.PasswordHash = _hasher.HashPassword(customer, vm.Password);
        customer.PasswordResetToken = null;
        customer.PasswordResetExpiresUtc = null;
        repo.Update(customer);
        await _uow.SaveChangesAsync(ct);
        return AuthResult.Success(customer.Id, customer.FullName, customer.Email, customer.IsAdmin, customer.IsVendor, customer.VendorId);
    }

    public async Task<AuthResult> LoginAsync(LoginVm vm, CancellationToken ct = default)
    {
        var email = vm.Email.Trim().ToLowerInvariant();
        var customer = await _uow.Repository<Customer>().FirstOrDefaultAsync(c => c.Email == email, ct);

        // Same generic message whether the email or password is wrong (avoids account enumeration).
        const string invalid = "Invalid email or password.";
        if (customer is null || string.IsNullOrEmpty(customer.PasswordHash))
            return AuthResult.Fail(invalid);
        if (!customer.IsActive)
            return AuthResult.Fail("This account has been deactivated.");

        var result = _hasher.VerifyHashedPassword(customer, customer.PasswordHash, vm.Password);
        if (result == PasswordVerificationResult.Failed)
            return AuthResult.Fail(invalid);

        // Transparently upgrade the hash if the algorithm/iterations changed.
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            customer.PasswordHash = _hasher.HashPassword(customer, vm.Password);
            _uow.Repository<Customer>().Update(customer);
            await _uow.SaveChangesAsync(ct);
        }

        var roles = await _uow.Repository<CustomerRole>().Query()
            .Where(cr => cr.CustomerId == customer.Id)
            .Select(cr => cr.Role.Name)
            .ToListAsync(ct);

        return AuthResult.Success(customer.Id, customer.FullName, customer.Email,
            customer.IsAdmin, customer.IsVendor, customer.VendorId, roles);
    }
}
