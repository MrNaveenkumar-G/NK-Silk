using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NKSilk.Application.Common.Interfaces;
using NKSilk.Domain.Common;
using NKSilk.Infrastructure.Data;

namespace NKSilk.Infrastructure.Repositories;

/// <summary>EF Core implementation of the generic repository.</summary>
public class Repository<T> : IRepository<T> where T : BaseEntity
{
    private readonly ApplicationDbContext _db;
    private readonly DbSet<T> _set;

    public Repository(ApplicationDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
    {
        IQueryable<T> q = _set.AsNoTracking();
        if (predicate is not null) q = q.Where(predicate);
        return await q.ToListAsync(ct);
    }

    public IQueryable<T> Query(bool asNoTracking = true)
        => asNoTracking ? _set.AsNoTracking() : _set;

    public Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default)
        => _set.FirstOrDefaultAsync(predicate, ct);

    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default)
        => predicate is null ? _set.CountAsync(ct) : _set.CountAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
        => await _set.AddAsync(entity, ct);

    public void Update(T entity) => _set.Update(entity);

    // Soft delete: flag the row; the global query filter hides it on subsequent reads.
    public void Remove(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAtUtc = DateTime.UtcNow;
        _set.Update(entity);
    }
}
