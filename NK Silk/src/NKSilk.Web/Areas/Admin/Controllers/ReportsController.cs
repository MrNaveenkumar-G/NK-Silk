using System.Text;
using Microsoft.AspNetCore.Mvc;
using NKSilk.Application.Services.Interfaces;

namespace NKSilk.Web.Areas.Admin.Controllers;

public class ReportsController : AdminBaseController
{
    private readonly IReportService _reports;
    public ReportsController(IReportService reports) => _reports = reports;

    public async Task<IActionResult> Index(int days = 30, CancellationToken ct = default)
    {
        ViewData["Days"] = days;
        return View(await _reports.GetSalesReportAsync(days, ct));
    }

    // Download the daily sales series as CSV.
    public async Task<IActionResult> Export(int days = 30, CancellationToken ct = default)
    {
        var report = await _reports.GetSalesReportAsync(days, ct);
        var sb = new StringBuilder();
        sb.AppendLine("Date,Orders,Revenue");
        foreach (var d in report.Daily)
            sb.AppendLine($"{d.Date:yyyy-MM-dd},{d.Orders},{d.Revenue}");
        sb.AppendLine();
        sb.AppendLine("Top Products,UnitsSold,Revenue");
        foreach (var p in report.TopProducts)
            sb.AppendLine($"\"{p.ProductName.Replace("\"", "'")}\",{p.UnitsSold},{p.Revenue}");

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", $"sales-report-{days}d.csv");
    }
}
