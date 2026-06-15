using System.Linq.Expressions;
using NKSilk.Domain.Common;

namespace NKSilk.Application.Common.Interfaces;

/// <summary>
/// Generic repository abstraction over an aggregate/entity type. Implemented in the
/// Infrastructure layer on top of EF Core so the Application layer stays persistence-agnostic.
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<IReadOnlyList<T>> ListAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    /// <summary>Returns an IQueryable for composing complex, projected queries in services.</summary>
    IQueryable<T> Query(bool asNoTracking = true);

    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);

    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    /// <summary>Soft delete — sets IsDeleted so the global query filter hides the row.</summary>
    void Remove(T entity);
}
