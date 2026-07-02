using Hangfire;
using MailKit.Net.Smtp;
using MailKit.Security;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NextErp.Application.DTOs;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Queries;
using SaleDto = NextErp.Application.DTOs.Sale;

namespace NextErp.Infrastructure.Services;

/// <summary>
/// MailKit-backed SMTP implementation of <see cref="IInvoiceEmailService"/>.
/// Provider-agnostic — the same code targets Resend, Mailtrap sandbox, Gmail
/// SMTP, or any RFC-compliant relay. Hangfire's retry policy handles
/// transient errors (the <see cref="AutomaticRetryAttribute"/> caps retries
/// so a permanently broken config doesn't fill the queue with retries
/// forever).
/// </summary>
public sealed class SmtpInvoiceEmailService(
    IMediator mediator,
    IInvoicePdfService invoicePdfService,
    IOptions<EmailOptions> options,
    ILogger<SmtpInvoiceEmailService> logger) : IInvoiceEmailService
{
    private readonly EmailOptions _options = options.Value;

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 600 })]
    public async Task SendSaleInvoiceAsync(
        Guid saleId,
        EmailMessageRequest message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (string.IsNullOrWhiteSpace(message.To))
            throw new ArgumentException("Recipient address is required.", nameof(message));

        // 1. Pull the sale + render the PDF inside the worker rather than the
        //    enqueue-time controller, so the job arg payload stays tiny.
        var sale = await mediator.Send(new GetSaleByIdQuery(saleId), cancellationToken)
            ?? throw new InvalidOperationException($"Sale {saleId} not found.");
        var dto = sale.ToResponse();
        var pdfBytes = await invoicePdfService.RenderSaleInvoiceAsync(dto, cancellationToken);
        var pdfFileName = $"invoice-{dto.SaleNumber}.pdf";

        // 2. Build the MIME message — text body + HTML body + PDF attachment.
        //    A multipart/alternative body keeps text-only mail clients happy.
        var mime = BuildMessage(dto, message, pdfBytes, pdfFileName);

        // 3. Kill-switch for environments without configured credentials. We
        //    log the intended recipient + size so debugging "where did my
        //    email go?" is one log search away.
        if (!_options.Enabled)
        {
            logger.LogInformation(
                "Email disabled — skipping send for sale {SaleId} to {Recipient} ({Bytes} bytes attachment)",
                saleId,
                message.To,
                pdfBytes.Length);
            return;
        }

        // 4. Connect → authenticate → send → quit. MailKit auto-picks the
        //    right TLS mode when given Auto, but we honour the explicit flag
        //    so behaviour is predictable across providers.
        using var smtp = new SmtpClient();
        var secure = _options.Smtp.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        try
        {
            await smtp.ConnectAsync(_options.Smtp.Host, _options.Smtp.Port, secure, cancellationToken);
            if (!string.IsNullOrEmpty(_options.Smtp.Username))
            {
                await smtp.AuthenticateAsync(_options.Smtp.Username, _options.Smtp.Password, cancellationToken);
            }
            await smtp.SendAsync(mime, cancellationToken);
            await smtp.DisconnectAsync(quit: true, cancellationToken);

            logger.LogInformation(
                "Sent invoice email for sale {SaleId} ({SaleNumber}) to {Recipient}",
                saleId,
                dto.SaleNumber,
                message.To);
        }
        catch (Exception ex)
        {
            // Re-throw so Hangfire records the failure + applies the retry
            // policy. Logging here gives us a per-attempt entry independent
            // of the dashboard view.
            logger.LogError(
                ex,
                "Failed to send invoice email for sale {SaleId} to {Recipient}",
                saleId,
                message.To);
            throw;
        }
    }

    private MimeMessage BuildMessage(
        SaleDto.SaleResponse sale,
        EmailMessageRequest req,
        byte[] pdfBytes,
        string pdfFileName)
    {
        var subject = string.IsNullOrWhiteSpace(req.Subject)
            ? $"Invoice {sale.SaleNumber}"
            : req.Subject!.Trim();

        var greeting = string.IsNullOrWhiteSpace(sale.CustomerName)
            ? "Hello,"
            : $"Hello {sale.CustomerName.Trim()},";

        // The user-supplied message is rendered as plain text wrapped in a
        // <pre>-like block; we deliberately don't allow raw HTML to keep
        // injection surface tight on a free-form input.
        var userMessageHtml = string.IsNullOrWhiteSpace(req.Message)
            ? string.Empty
            : $"<p style=\"white-space:pre-wrap\">{System.Net.WebUtility.HtmlEncode(req.Message)}</p>";

        var amountText = sale.FinalAmount.ToString("N2");
        var html = $$"""
            <!DOCTYPE html>
            <html><body style="font-family:Arial,sans-serif;line-height:1.4;color:#222">
              <p>{{System.Net.WebUtility.HtmlEncode(greeting)}}</p>
              {{userMessageHtml}}
              <p>Please find attached invoice <strong>{{System.Net.WebUtility.HtmlEncode(sale.SaleNumber)}}</strong> for the amount of <strong>{{amountText}}</strong>.</p>
              <p style="color:#666;font-size:12px;margin-top:24px">— Sent via NextErp</p>
            </body></html>
            """;

        var text =
            $"{greeting}\n\n" +
            (string.IsNullOrWhiteSpace(req.Message) ? "" : $"{req.Message!.Trim()}\n\n") +
            $"Please find attached invoice {sale.SaleNumber} for the amount of {amountText}.\n\n— Sent via NextErp";

        var builder = new BodyBuilder
        {
            HtmlBody = html,
            TextBody = text,
        };
        builder.Attachments.Add(pdfFileName, pdfBytes, new ContentType("application", "pdf"));

        var message = new MimeMessage
        {
            Subject = subject,
            Body = builder.ToMessageBody(),
        };
        message.From.Add(MailboxAddress.Parse(_options.From));
        message.To.Add(MailboxAddress.Parse(req.To.Trim()));

        if (!string.IsNullOrWhiteSpace(req.Cc))
        {
            foreach (var cc in req.Cc.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                message.Cc.Add(MailboxAddress.Parse(cc));
            }
        }

        if (!string.IsNullOrWhiteSpace(_options.ReplyTo))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(_options.ReplyTo!));
        }

        return message;
    }
}
