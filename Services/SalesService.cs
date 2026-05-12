using Microsoft.EntityFrameworkCore;
using VehicleParts.API.Data;
using VehicleParts.API.Models;
using VehicleParts.API.ViewModels;

namespace VehicleParts.API.Services;

public class SalesService : ISalesService
{
    private readonly ApplicationDbContext _context;

    public SalesService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateSaleAsync(SaleCreateViewModel model, CancellationToken cancellationToken = default)
    {
        if (model.Items.Count == 0)
            throw new ArgumentException("At least one item is required.");

        if (model.CustomerId <= 0)
            throw new ArgumentException("Valid CustomerId is required.");

        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Id == model.CustomerId, cancellationToken);

        if (customer == null)
            throw new KeyNotFoundException("Customer not found");

        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        var sale = new Sale
        {
            CustomerId = customer.Id,
            StaffId = model.StaffId,
            PaymentMethod = string.IsNullOrWhiteSpace(model.PaymentMethod) ? "Cash" : model.PaymentMethod.Trim(),
            AmountPaid = model.AmountPaid
        };

        var items = new List<SaleItem>();
        decimal subtotal = 0m;

        foreach (var item in model.Items)
        {
            if (item.PartId <= 0)
                throw new ArgumentException("Valid PartId is required.");

            if (item.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0.");

            var part = await _context.Parts.FirstOrDefaultAsync(p => p.Id == item.PartId, cancellationToken);
            if (part == null)
                throw new KeyNotFoundException($"Part not found (Id={item.PartId})");

            if (part.StockQuantity < item.Quantity)
                throw new InvalidOperationException($"Insufficient stock for part {part.Name} (available={part.StockQuantity})");

            part.StockQuantity -= item.Quantity;

            var unitPrice = part.Price;
            var lineTotal = unitPrice * item.Quantity;

            items.Add(new SaleItem
            {
                PartId = part.Id,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                LineTotal = lineTotal
            });

            subtotal += lineTotal;
        }

        var discount = CalculateLoyaltyDiscount(subtotal);
        var total = subtotal - discount;

        sale.Subtotal = subtotal;
        sale.DiscountAmount = discount;
        sale.TotalAmount = total;

        if (sale.AmountPaid < 0)
            throw new ArgumentException("AmountPaid cannot be negative.");

        if (sale.AmountPaid > sale.TotalAmount)
            throw new ArgumentException("AmountPaid cannot exceed TotalAmount.");

        if (sale.PaymentMethod.Equals("Credit", StringComparison.OrdinalIgnoreCase))
        {
            sale.CreditDueDate = model.CreditDueDate ?? sale.SaleDate.AddDays(30);
        }
        else
        {
            sale.CreditDueDate = null;
        }

        sale.Items = items;

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return sale;
    }

    private static decimal CalculateLoyaltyDiscount(decimal subtotal)
    {
        // Loyalty Program: 10% discount if spending more than 5000 in a single purchase.
        if (subtotal > 5000m)
            return Math.Round(subtotal * 0.10m, 2);

        return 0m;
    }
}
