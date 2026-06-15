using System.Collections.Concurrent;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Domain.Common;
using NKSilk.Infrastructure.Data;

namespace NKSilk.Infrastructure.Repositories;

/// <summary>
/// Wraps a single DbContext. Repositories are cached per entity type so they all
/// participate in the same change tracker and commit together.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public UnitOfWork(ApplicationDbContext db) => _db = db;

    public IRepository<T> Repository<T>() where T : BaseEntity
        => (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(_db));

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

    public async ValueTask DisposeAsync() => await _db.DisposeAsync();
}
