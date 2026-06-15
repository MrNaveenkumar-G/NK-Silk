using NKSilk.Application.ViewModels;

namespace NKSilk.Application.Services.Interfaces;

public interface IReportService
{
    /// <summary>Sales analytics over the trailing <paramref name="days"/> window.</summary>
    Task<SalesReportVm> GetSalesReportAsync(int days, CancellationToken ct = default);
}
