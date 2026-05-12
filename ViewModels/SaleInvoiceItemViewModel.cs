namespace VehicleParts.API.ViewModels;

public class SaleInvoiceItemViewModel
{
    public int PartId { get; set; }
    public string PartName { get; set; } = "";
    public string PartNumber { get; set; } = "";

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
