namespace NextErp.Domain.Entities;

/// <summary>
/// Tags the business event that triggered a journal entry. Drives the
/// auto-journal hook in command handlers (e.g. when a sale is created we
/// emit a JournalEntry with ReferenceType=Sale + ReferenceId=saleId).
/// </summary>
public enum JournalEntryReferenceType
{
    Manual = 1,
    Sale = 2,
    Purchase = 3,
    SalePayment = 4,
    PurchasePayment = 5,
    /// <summary>Account-to-account transfer (the "transaction from any account to any account" feature).</summary>
    Transfer = 6,
    StockAdjustment = 7,
    Opening = 8,
}
