using Hangfire;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using NextErp.Application.Interfaces;
using NextErp.Domain.Entities;

namespace NextErp.Infrastructure.Services;

/// <summary>
/// MailKit-backed implementation of <see cref="ICustomerBroadcastEmailService"/>.
/// Shares the same <see cref="EmailOptions"/> (and so the same SMTP credentials)
/// as the invoice service — there's no good reason for marketing/notification
/// blasts to live on a different relay in a single-tenant ERP.
///
/// We resolve the customer's email at send time (not enqueue time) so an
/// admin who fixes a typo in the customer record between scheduling and
/// sending sees the corrected address used.
/// </summary>
public sealed class SmtpCustomerBroadcastEmailService(
    IApplicationDbContext db,
    IOptions<EmailOptions> options,
    ILogger<SmtpCustomerBroadcastEmailService> logger) : ICustomerBroadcastEmailService
{
    private readonly EmailOptions _options = options.Value;

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 30, 120, 600 })]
    public async Task SendBroadcastAsync(
        Guid customerId,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.", nameof(body));

        // We deliberately bypass the branch query filter — a Hangfire worker
        // runs without an HttpContext and so has no branch in scope. The
        // BranchScopedQueryFilter falls back to "no scope" in that case, so
        // .IgnoreQueryFilters() is only a belt-and-braces here.
        var customer = await db.Parties
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                p => p.Id == customerId && p.PartyType == PartyType.Customer,
                cancellationToken);

        if (customer == null)
        {
            logger.LogWarning(
                "Bulk-email job: customer {CustomerId} not found — skipping.",
                customerId);
            return;
        }

        if (string.IsNullOrWhiteSpace(customer.Email))
        {
            logger.LogInformation(
                "Bulk-email job: customer {CustomerId} ({Name}) has no email — skipping.",
                customerId,
                customer.Title);
            return;
        }

        var mime = BuildMessage(customer, subject, body);

        if (!_options.Enabled)
        {
            // Same kill-switch as the invoice service. Lets us run the full
            // pipeline in dev without burning Mailtrap throughput.
            logger.LogInformation(
                "Email disabled — skipping broadcast for customer {CustomerId} to {Recipient}",
                customerId,
                customer.Email);
            return;
        }

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
                "Sent broadcast email to customer {CustomerId} ({Recipient})",
                customerId,
                customer.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to send broadcast email to customer {CustomerId} ({Recipient})",
                customerId,
                customer.Email);
            throw;
        }
    }

    private MimeMessage BuildMessage(Party customer, string subject, string body)
    {
        var greeting = string.IsNullOrWhiteSpace(customer.Title)
            ? "Hello,"
            : $"Hello {customer.Title.Trim()},";

        // We HTML-encode the user-supplied body and wrap in a single <pre>-style
        // paragraph that respects line breaks. Letting raw HTML through would
        // mean an operator could accidentally (or deliberately) inject markup
        // that gets sent to the entire customer base.
        var html = $$"""
            <!DOCTYPE html>
            <html><body style="font-family:Arial,sans-serif;line-height:1.4;color:#222">
              <p>{{System.Net.WebUtility.HtmlEncode(greeting)}}</p>
              <p style="white-space:pre-wrap">{{System.Net.WebUtility.HtmlEncode(body)}}</p>
              <p style="color:#666;font-size:12px;margin-top:24px">— Sent via NextErp</p>
            </body></html>
            """;

        var text = $"{greeting}\n\n{body}\n\n— Sent via NextErp";

        var builder = new BodyBuilder
        {
            HtmlBody = html,
            TextBody = text,
        };

        var message = new MimeMessage
        {
            Subject = subject.Trim(),
            Body = builder.ToMessageBody(),
        };
        message.From.Add(MailboxAddress.Parse(_options.From));
        message.To.Add(MailboxAddress.Parse(customer.Email!.Trim()));

        if (!string.IsNullOrWhiteSpace(_options.ReplyTo))
        {
            message.ReplyTo.Add(MailboxAddress.Parse(_options.ReplyTo!));
        }

        return message;
    }
}
