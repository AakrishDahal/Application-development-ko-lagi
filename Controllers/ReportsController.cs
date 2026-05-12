using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.ViewModels;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ReportsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("high-spenders")]
    public async Task<IActionResult> GetHighSpenders([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] int top = 10, CancellationToken cancellationToken = default)
    {
        if (top <= 0) top = 10;

        var query = _context.Sales.AsQueryable();

        if (from.HasValue)
            query = query.Where(s => s.SaleDate >= from.Value);
        if (to.HasValue)
            query = query.Where(s => s.SaleDate <= to.Value);

        var report = await (
            from s in query
            group s by s.CustomerId
            into g
            join c in _context.Customers on g.Key equals c.Id
            orderby g.Sum(x => x.TotalAmount) descending
            select new HighSpenderReportItemViewModel
            {
                CustomerId = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                PurchaseCount = g.Count(),
                TotalSpent = g.Sum(x => x.TotalAmount)
            }
        )
        .Take(top)
        .ToListAsync(cancellationToken);

        return Ok(report);
    }

    [HttpGet("regular-customers")]
    public async Task<IActionResult> GetRegularCustomers([FromQuery] int minPurchases = 2, CancellationToken cancellationToken = default)
    {
        if (minPurchases <= 0) minPurchases = 1;

        var report = await (
            from s in _context.Sales
            group s by s.CustomerId
            into g
            where g.Count() >= minPurchases
            join c in _context.Customers on g.Key equals c.Id
            orderby g.Count() descending
            select new RegularCustomerReportItemViewModel
            {
                CustomerId = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                PurchaseCount = g.Count(),
                LastPurchaseDate = g.Max(x => x.SaleDate)
            }
        )
        .ToListAsync(cancellationToken);

        return Ok(report);
    }

    [HttpGet("pending-credits")]
    public async Task<IActionResult> GetPendingCredits([FromQuery] bool overdueOnly = false, [FromQuery] DateTime? asOf = null, CancellationToken cancellationToken = default)
    {
        var asOfDate = asOf ?? DateTime.UtcNow;

        var creditSales = _context.Sales
            .Where(s => s.PaymentMethod.ToLower() == "credit")
            .Where(s => (s.TotalAmount - s.AmountPaid) > 0);

        if (overdueOnly)
            creditSales = creditSales.Where(s => s.CreditDueDate != null && s.CreditDueDate < asOfDate);

        var report = await (
            from s in creditSales
            join c in _context.Customers on s.CustomerId equals c.Id
            orderby s.CreditDueDate
            select new PendingCreditReportItemViewModel
            {
                CustomerId = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                SaleId = s.Id,
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount,
                AmountPaid = s.AmountPaid,
                OutstandingAmount = s.TotalAmount - s.AmountPaid,
                CreditDueDate = s.CreditDueDate,
                IsOverdue = s.CreditDueDate != null && s.CreditDueDate < asOfDate
            }
        )
        .ToListAsync(cancellationToken);

        return Ok(report);
    }
}
