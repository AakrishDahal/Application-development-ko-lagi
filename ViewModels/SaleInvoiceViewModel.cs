namespace VehicleParts.API.ViewModels;

public class SaleInvoiceViewModel
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }

    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";

    public int? StaffId { get; set; }

    public string PaymentMethod { get; set; } = "";

    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }

    public DateTime? CreditDueDate { get; set; }

    public List<SaleInvoiceItemViewModel> Items { get; set; } = new();
}
