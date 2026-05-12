namespace VehicleParts.API.ViewModels;

public class SaleCreateViewModel
{
    public int CustomerId { get; set; }

    public int? StaffId { get; set; }

    // Cash | Card | Credit
    public string PaymentMethod { get; set; } = "Cash";

    public decimal AmountPaid { get; set; }

    // Only used when PaymentMethod == Credit; defaults to SaleDate + 30 days if omitted.
    public DateTime? CreditDueDate { get; set; }

    public bool SendEmail { get; set; }

    // If provided, sends invoice to this email instead of the customer's stored email.
    public string? EmailOverride { get; set; }

    public List<SaleCreateItemViewModel> Items { get; set; } = new();
}
