using NKSilk.Domain.Common;

namespace NKSilk.Application.Common.Interfaces;

/// <summary>
/// Coordinates a single business transaction. Hands out repositories that share one
/// DbContext and commits them together via SaveChangesAsync.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IRepository<T> Repository<T>() where T : BaseEntity;
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
