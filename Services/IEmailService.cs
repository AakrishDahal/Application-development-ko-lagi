using VehicleParts.API.Models;

namespace VehicleParts.API.Services;

public interface IEmailService
{
    Task SendInvoiceEmailAsync(Sale sale, string toEmail, string emailBody, CancellationToken cancellationToken = default);
}
