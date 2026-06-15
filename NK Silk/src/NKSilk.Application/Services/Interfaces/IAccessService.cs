using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IAccessService
{
    // Roles / RBAC
    Task<IReadOnlyList<RoleVm>> GetRolesAsync(CancellationToken ct = default);
    Task<CustomerAccessVm?> GetCustomerAccessAsync(int customerId, CancellationToken ct = default);
    Task<AdminResult> SetCustomerRolesAsync(int customerId, IReadOnlyList<string> roleNames, CancellationToken ct = default);
    Task<IReadOnlyList<string>> GetRoleNamesForCustomerAsync(int customerId, CancellationToken ct = default);

    // Audit trail
    Task<IReadOnlyList<AuditLogVm>> GetRecentAuditAsync(string? entityName, int take = 200, CancellationToken ct = default);
}
