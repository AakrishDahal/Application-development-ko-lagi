using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.Models;
using VehicleParts.API.Services;
using VehicleParts.API.ViewModels;

namespace VehicleParts.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SalesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ISalesService _salesService;
    private readonly IEmailService _emailService;

    public SalesController(ApplicationDbContext context, ISalesService salesService, IEmailService emailService)
    {
        _context = context;
        _salesService = salesService;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateSale([FromBody] SaleCreateViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            var created = await _salesService.CreateSaleAsync(model, cancellationToken);

            var sale = await LoadSaleInvoiceAsync(created.Id, cancellationToken);
            if (sale == null)
                return NotFound(new { message = "Sale not found" });

            var invoice = MapToInvoiceViewModel(sale);

            if (model.SendEmail)
            {
                var to = string.IsNullOrWhiteSpace(model.EmailOverride) ? sale.Customer?.Email : model.EmailOverride;
                if (string.IsNullOrWhiteSpace(to))
                    return BadRequest(new { message = "Customer email not found and EmailOverride not provided." });

                var body = BuildInvoiceEmailBody(invoice);
                await _emailService.SendInvoiceEmailAsync(sale, to!, body, cancellationToken);
            }

            return Ok(invoice);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvoice(int id, CancellationToken cancellationToken)
    {
        var sale = await LoadSaleInvoiceAsync(id, cancellationToken);
        if (sale == null)
            return NotFound(new { message = "Sale not found" });

        return Ok(MapToInvoiceViewModel(sale));
    }

    [HttpPost("{id:int}/email")]
    public async Task<IActionResult> SendInvoiceEmail(int id, [FromQuery] string? toEmail, CancellationToken cancellationToken)
    {
        var sale = await LoadSaleInvoiceAsync(id, cancellationToken);
        if (sale == null)
            return NotFound(new { message = "Sale not found" });

        var invoice = MapToInvoiceViewModel(sale);
        var to = string.IsNullOrWhiteSpace(toEmail) ? sale.Customer?.Email : toEmail;

        if (string.IsNullOrWhiteSpace(to))
            return BadRequest(new { message = "Customer email not found and toEmail not provided." });

        var body = BuildInvoiceEmailBody(invoice);
        await _emailService.SendInvoiceEmailAsync(sale, to!, body, cancellationToken);

        return Ok(new { message = "Invoice email sent (or logged in Development)." });
    }

    private Task<Sale?> LoadSaleInvoiceAsync(int saleId, CancellationToken cancellationToken)
    {
        return _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.Items)
            .ThenInclude(i => i.Part)
            .FirstOrDefaultAsync(s => s.Id == saleId, cancellationToken);
    }

    private static SaleInvoiceViewModel MapToInvoiceViewModel(Sale sale)
    {
        var invoice = new SaleInvoiceViewModel
        {
            SaleId = sale.Id,
            SaleDate = sale.SaleDate,
            CustomerId = sale.CustomerId,
            CustomerName = sale.Customer?.FullName ?? "",
            CustomerEmail = sale.Customer?.Email ?? "",
            StaffId = sale.StaffId,
            PaymentMethod = sale.PaymentMethod,
            Subtotal = sale.Subtotal,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            AmountPaid = sale.AmountPaid,
            OutstandingAmount = Math.Max(0m, sale.TotalAmount - sale.AmountPaid),
            CreditDueDate = sale.CreditDueDate
        };

        foreach (var item in sale.Items.OrderBy(i => i.Id))
        {
            invoice.Items.Add(new SaleInvoiceItemViewModel
            {
                PartId = item.PartId,
                PartName = item.Part?.Name ?? "",
                PartNumber = item.Part?.PartNumber ?? "",
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            });
        }

        return invoice;
    }

    private static string BuildInvoiceEmailBody(SaleInvoiceViewModel invoice)
    {
        var lines = new List<string>
        {
            $"Invoice #{invoice.SaleId}",
            $"Date: {invoice.SaleDate:u}",
            $"Customer: {invoice.CustomerName} (#{invoice.CustomerId})",
            $"Payment Method: {invoice.PaymentMethod}",
            "",
            "Items:"
        };

        foreach (var item in invoice.Items)
        {
            lines.Add($"- {item.PartName} ({item.PartNumber}) x{item.Quantity} @ {item.UnitPrice} = {item.LineTotal}");
        }

        lines.Add("");
        lines.Add($"Subtotal: {invoice.Subtotal}");
        lines.Add($"Discount: {invoice.DiscountAmount}");
        lines.Add($"Total: {invoice.TotalAmount}");
        lines.Add($"Paid: {invoice.AmountPaid}");
        lines.Add($"Outstanding: {invoice.OutstandingAmount}");

        if (invoice.PaymentMethod.Equals("Credit", StringComparison.OrdinalIgnoreCase))
            lines.Add($"Credit Due Date: {invoice.CreditDueDate:u}");

        return string.Join(Environment.NewLine, lines);
    }
}
