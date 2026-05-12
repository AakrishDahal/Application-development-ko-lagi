using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.ViewModels;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCustomerDetails(int id, CancellationToken cancellationToken)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (customer == null)
            return NotFound(new { message = "Customer not found" });

        var vehicles = await _context.Vehicles
            .Where(v => v.CustomerId == id)
            .OrderBy(v => v.Id)
            .Select(v => new VehicleSummaryViewModel
            {
                Id = v.Id,
                VehicleNumber = v.VehicleNumber,
                VehicleModel = v.VehicleModel,
                VehicleBrand = v.VehicleBrand
            })
            .ToListAsync(cancellationToken);

        var saleStats = await _context.Sales
            .Where(s => s.CustomerId == id)
            .GroupBy(s => s.CustomerId)
            .Select(g => new
            {
                PurchaseCount = g.Count(),
                TotalSpent = g.Sum(x => x.TotalAmount),
                OutstandingCredit = g.Sum(x => (x.TotalAmount - x.AmountPaid) > 0 ? (x.TotalAmount - x.AmountPaid) : 0)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var response = new CustomerDetailsViewModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Vehicles = vehicles,
            PurchaseCount = saleStats?.PurchaseCount ?? 0,
            TotalSpent = saleStats?.TotalSpent ?? 0m,
            OutstandingCredit = saleStats?.OutstandingCredit ?? 0m
        };

        return Ok(response);
    }

    [HttpGet("{id:int}/sales")]
    public async Task<IActionResult> GetCustomerSalesHistory(int id, CancellationToken cancellationToken)
    {
        var customerExists = await _context.Customers.AnyAsync(c => c.Id == id, cancellationToken);
        if (!customerExists)
            return NotFound(new { message = "Customer not found" });

        var sales = await _context.Sales
            .Where(s => s.CustomerId == id)
            .OrderByDescending(s => s.SaleDate)
            .Select(s => new CustomerSaleSummaryViewModel
            {
                SaleId = s.Id,
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount,
                DiscountAmount = s.DiscountAmount,
                PaymentMethod = s.PaymentMethod,
                AmountPaid = s.AmountPaid,
                OutstandingAmount = (s.TotalAmount - s.AmountPaid) > 0 ? (s.TotalAmount - s.AmountPaid) : 0,
                CreditDueDate = s.CreditDueDate
            })
            .ToListAsync(cancellationToken);

        return Ok(sales);
    }
}
