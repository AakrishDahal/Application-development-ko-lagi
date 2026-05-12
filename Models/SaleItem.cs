namespace VehicleParts.API.Models;

public class SaleItem
{
    public int Id { get; set; }

    public int SaleId { get; set; }
    public Sale? Sale { get; set; }

    public int PartId { get; set; }
    public Part? Part { get; set; }

    public int Quantity { get; set; }

    // Snapshot of Part.Price at sale time
    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }
}
