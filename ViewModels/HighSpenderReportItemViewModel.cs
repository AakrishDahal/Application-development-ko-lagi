namespace VehicleParts.API.ViewModels;

public class HighSpenderReportItemViewModel
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    public int PurchaseCount { get; set; }
    public decimal TotalSpent { get; set; }
}
