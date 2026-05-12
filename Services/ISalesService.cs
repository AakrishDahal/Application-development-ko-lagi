using VehicleParts.API.Models;
using VehicleParts.API.ViewModels;

namespace VehicleParts.API.Services;

public interface ISalesService
{
    Task<Sale> CreateSaleAsync(SaleCreateViewModel model, CancellationToken cancellationToken = default);
}
