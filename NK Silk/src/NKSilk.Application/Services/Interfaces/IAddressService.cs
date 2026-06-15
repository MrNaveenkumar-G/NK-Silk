using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IAddressService
{
    Task<IReadOnlyList<AddressVm>> GetForCustomerAsync(int customerId, CancellationToken ct = default);
    Task<AddressFormVm?> GetForEditAsync(int customerId, int? id, CancellationToken ct = default);
    Task<int> SaveAsync(int customerId, AddressFormVm vm, CancellationToken ct = default);
    Task DeleteAsync(int customerId, int id, CancellationToken ct = default);
    Task SetDefaultAsync(int customerId, int id, CancellationToken ct = default);
}
