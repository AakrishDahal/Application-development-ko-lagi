using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using VehicleParts.API.Models;

namespace VehicleParts.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task SendInvoiceEmailAsync(Sale sale, string toEmail, string emailBody, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Smtp:Host"];
        var portString = _configuration["Smtp:Port"];
        var user = _configuration["Smtp:Username"];
        var pass = _configuration["Smtp:Password"];
        var from = _configuration["Smtp:From"];
        var enableSslString = _configuration["Smtp:EnableSsl"];

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            if (_environment.IsDevelopment())
            {
                _logger.LogInformation("SMTP not configured. DEV fallback: would send invoice email to {To}. Subject: {Subject}. Body:\n{Body}",
                    toEmail, $"Invoice #{sale.Id}", emailBody);
                return;
            }

            throw new InvalidOperationException("SMTP is not configured. Set Smtp:Host and Smtp:From in appsettings.");
        }

        _ = int.TryParse(portString, out var port);
        if (port <= 0)
            port = 587;

        var enableSsl = true;
        if (!string.IsNullOrWhiteSpace(enableSslString) && bool.TryParse(enableSslString, out var parsedSsl))
            enableSsl = parsedSsl;

        using var message = new MailMessage(from, toEmail)
        {
            Subject = $"Invoice #{sale.Id}",
            Body = emailBody,
            IsBodyHtml = false
        };

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        if (!string.IsNullOrWhiteSpace(user))
            client.Credentials = new NetworkCredential(user, pass);

        // SmtpClient doesn't support CancellationToken directly.
        await client.SendMailAsync(message);
    }
}
