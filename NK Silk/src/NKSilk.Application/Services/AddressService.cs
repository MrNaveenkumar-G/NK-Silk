using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Application.Services.Interfaces;
using NKSilk.Application.ViewModels;
using NKSilk.Domain.Entities;

namespace NKSilk.Application.Services;

/// <summary>Customer address book — reusable saved shipping addresses.</summary>
public class AddressService : IAddressService
{
    private readonly IUnitOfWork _uow;
    public AddressService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<AddressVm>> GetForCustomerAsync(int customerId, CancellationToken ct = default)
        => await _uow.Repository<Address>().Query()
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefault).ThenByDescending(a => a.CreatedAtUtc)
            .Select(a => new AddressVm
            {
                Id = a.Id,
                ContactName = a.ContactName,
                PhoneNumber = a.PhoneNumber,
                Line1 = a.Line1,
                Line2 = a.Line2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault
            }).ToListAsync(ct);

    public async Task<AddressFormVm?> GetForEditAsync(int customerId, int? id, CancellationToken ct = default)
    {
        if (id is null or 0) return new AddressFormVm();
        return await _uow.Repository<Address>().Query()
            .Where(a => a.Id == id && a.CustomerId == customerId)
            .Select(a => new AddressFormVm
            {
                Id = a.Id,
                ContactName = a.ContactName,
                PhoneNumber = a.PhoneNumber,
                Line1 = a.Line1,
                Line2 = a.Line2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault
            }).FirstOrDefaultAsync(ct);
    }

    public async Task<int> SaveAsync(int customerId, AddressFormVm vm, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Address>();
        Address entity;
        if (vm.Id == 0)
        {
            entity = new Address { CustomerId = customerId, Country = "India", CreatedAtUtc = DateTime.UtcNow };
            Apply(entity, vm);
            await repo.AddAsync(entity, ct);
        }
        else
        {
            entity = await repo.Query(asNoTracking: false)
                .FirstOrDefaultAsync(a => a.Id == vm.Id && a.CustomerId == customerId, ct)
                ?? throw new InvalidOperationException("Address not found.");
            Apply(entity, vm);
            repo.Update(entity);
        }
        await _uow.SaveChangesAsync(ct);

        if (vm.IsDefault) await SetDefaultAsync(customerId, entity.Id, ct);
        return entity.Id;

        static void Apply(Address a, AddressFormVm vm)
        {
            a.ContactName = vm.ContactName.Trim();
            a.PhoneNumber = vm.PhoneNumber.Trim();
            a.Line1 = vm.Line1.Trim();
            a.Line2 = string.IsNullOrWhiteSpace(vm.Line2) ? null : vm.Line2.Trim();
            a.City = vm.City.Trim();
            a.State = vm.State.Trim();
            a.PostalCode = vm.PostalCode.Trim();
            a.IsDefault = vm.IsDefault;
        }
    }

    public async Task DeleteAsync(int customerId, int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Address>();
        var a = await repo.Query(asNoTracking: false).FirstOrDefaultAsync(x => x.Id == id && x.CustomerId == customerId, ct);
        if (a is null) return;
        repo.Remove(a);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task SetDefaultAsync(int customerId, int id, CancellationToken ct = default)
    {
        var repo = _uow.Repository<Address>();
        var all = await repo.Query(asNoTracking: false).Where(a => a.CustomerId == customerId).ToListAsync(ct);
        var changed = false;
        foreach (var a in all)
        {
            var shouldBeDefault = a.Id == id;
            if (a.IsDefault != shouldBeDefault) { a.IsDefault = shouldBeDefault; changed = true; }
        }
        if (changed) await _uow.SaveChangesAsync(ct);
    }
}
