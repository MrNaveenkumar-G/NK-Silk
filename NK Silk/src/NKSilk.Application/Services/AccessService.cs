using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

/// <summary>Role assignment (RBAC) and read access to the audit trail.</summary>
public class AccessService : IAccessService
{
    private readonly IUnitOfWork _uow;
    public AccessService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<RoleVm>> GetRolesAsync(CancellationToken ct = default)
        => await _uow.Repository<Role>().Query()
            .OrderBy(r => r.Name)
            .Select(r => new RoleVm
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                MemberCount = r.CustomerRoles.Count
            }).ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetRoleNamesForCustomerAsync(int customerId, CancellationToken ct = default)
        => await _uow.Repository<CustomerRole>().Query()
            .Where(cr => cr.CustomerId == customerId)
            .Select(cr => cr.Role.Name)
            .ToListAsync(ct);

    public async Task<CustomerAccessVm?> GetCustomerAccessAsync(int customerId, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query()
            .Where(c => c.Id == customerId)
            .Select(c => new { c.Id, c.FullName, c.Email })
            .FirstOrDefaultAsync(ct);
        if (customer is null) return null;

        return new CustomerAccessVm
        {
            CustomerId = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            AllRoles = await GetRolesAsync(ct),
            AssignedRoles = await GetRoleNamesForCustomerAsync(customerId, ct)
        };
    }

    public async Task<AdminResult> SetCustomerRolesAsync(int customerId, IReadOnlyList<string> roleNames, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query(asNoTracking: false)
            .FirstOrDefaultAsync(c => c.Id == customerId, ct);
        if (customer is null) return AdminResult.Fail("Customer not found.");

        var roles = await _uow.Repository<Role>().Query().ToListAsync(ct);
        var wanted = roles.Where(r => roleNames.Contains(r.Name)).ToList();

        var crRepo = _uow.Repository<CustomerRole>();
        var current = await crRepo.Query(asNoTracking: false)
            .Where(cr => cr.CustomerId == customerId).ToListAsync(ct);

        // Remove unwanted.
        foreach (var cr in current.Where(cr => wanted.All(w => w.Id != cr.RoleId)))
            crRepo.Remove(cr);
        // Add missing.
        foreach (var w in wanted.Where(w => current.All(cr => cr.RoleId != w.Id)))
            await crRepo.AddAsync(new CustomerRole { CustomerId = customerId, RoleId = w.Id, CreatedAtUtc = DateTime.UtcNow }, ct);

        // Keep the convenience flags in step with the Admin/Vendor roles.
        customer.IsAdmin = wanted.Any(w => w.Name == "Admin");
        customer.IsVendor = wanted.Any(w => w.Name == "Vendor");
        _uow.Repository<Customer>().Update(customer);

        await _uow.SaveChangesAsync(ct);
        return AdminResult.Ok(customerId);
    }

    public async Task<IReadOnlyList<AuditLogVm>> GetRecentAuditAsync(string? entityName, int take = 200, CancellationToken ct = default)
    {
        if (take is < 1 or > 1000) take = 200;
        var q = _uow.Repository<AuditLog>().Query();
        if (!string.IsNullOrWhiteSpace(entityName))
            q = q.Where(a => a.EntityName == entityName);

        return await q.OrderByDescending(a => a.CreatedAtUtc).Take(take)
            .Select(a => new AuditLogVm
            {
                Id = a.Id,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                UserName = a.UserName,
                Details = a.Details,
                CreatedAtUtc = a.CreatedAtUtc
            }).ToListAsync(ct);
    }
}
