namespace VehicleParts.API.ViewModels;

public class CustomerDetailsViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public string Email { get; set; } = "";

    public List<VehicleSummaryViewModel> Vehicles { get; set; } = new();

    public int PurchaseCount { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal OutstandingCredit { get; set; }
}
