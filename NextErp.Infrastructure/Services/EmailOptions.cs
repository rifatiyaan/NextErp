namespace NextErp.Infrastructure.Services;

/// <summary>
/// Strongly-typed options bound to the <c>Email</c> section of appsettings.
/// Kept inside the Infrastructure layer because SMTP transport details are
/// an implementation concern — Application code shouldn't need to read it.
/// </summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>
    /// Master kill-switch. When false, <c>SmtpInvoiceEmailService</c> logs
    /// the would-be send and returns successfully without touching SMTP —
    /// useful in dev / CI without configured credentials.
    /// </summary>
    public bool Enabled { get; init; }

    public SmtpOptions Smtp { get; init; } = new();

    /// <summary>RFC 5322 mailbox ("Display Name &lt;address@host&gt;" or just "address@host").</summary>
    public string From { get; init; } = "NextErp <noreply@example.com>";

    public string? ReplyTo { get; init; }
}

public sealed class SmtpOptions
{
    public string Host { get; init; } = "localhost";
    public int Port { get; init; } = 587;
    public string Username { get; init; } = "";
    public string Password { get; init; } = "";

    /// <summary>
    /// When true, connect with STARTTLS upgrade (port 587). When false, the
    /// transport falls back to MailKit's auto-detect — works with Mailtrap's
    /// 587 + Resend's 587/465 + Gmail's 587 without further branching.
    /// </summary>
    public bool UseStartTls { get; init; } = true;
}
