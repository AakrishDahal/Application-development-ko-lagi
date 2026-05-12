namespace VehicleParts.API.Models;

public class Sale
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Stored as plain int (no FK) to avoid conflicts with future Staff/Auth implementation.
    public int? StaffId { get; set; }

    public DateTime SaleDate { get; set; } = DateTime.UtcNow;

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    // Cash | Card | Credit
    public string PaymentMethod { get; set; } = "Cash";

    public decimal AmountPaid { get; set; }
    public DateTime? CreditDueDate { get; set; }

    public List<SaleItem> Items { get; set; } = new();
}
