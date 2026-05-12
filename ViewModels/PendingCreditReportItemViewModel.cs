namespace VehicleParts.API.ViewModels;

public class PendingCreditReportItemViewModel
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }

    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public decimal OutstandingAmount { get; set; }

    public DateTime? CreditDueDate { get; set; }
    public bool IsOverdue { get; set; }
}
