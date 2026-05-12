namespace VehicleParts.API.ViewModels;

public class RegularCustomerReportItemViewModel
{
    public int CustomerId { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    public int PurchaseCount { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
}
