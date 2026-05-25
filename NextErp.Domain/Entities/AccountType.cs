namespace NextErp.Domain.Entities;

/// <summary>
/// Top-level classification of a chart-of-accounts entry. Drives the sign
/// convention for balance display + which financial statement the account
/// rolls up into (Asset/Liability/Equity → balance sheet; Revenue/Expense
/// → income statement).
/// </summary>
public enum AccountType
{
    Asset = 1,
    Liability = 2,
    Equity = 3,
    Revenue = 4,
    Expense = 5,
}
