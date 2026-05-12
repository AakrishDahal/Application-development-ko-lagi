namespace VehicleParts.API.ViewModels;

public class CustomerSaleSummaryViewModel
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }

    public string PaymentMethod { get; set; } = "";
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }

    public DateTime? CreditDueDate { get; set; }
}
