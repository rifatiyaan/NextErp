using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Accounting;

public partial class JournalDto
{
    public partial class Request
    {
        /// <summary>
        /// Account-to-account transfer. Backend converts this to a balanced
        /// JournalEntry with two lines (debit destination, credit source).
        /// </summary>
        public class Transfer
        {
            public Guid FromAccountId { get; set; }
            public Guid ToAccountId { get; set; }
            public decimal Amount { get; set; }
            public string Description { get; set; } = null!;
            public DateTime? EntryDate { get; set; }
            public string? Reference { get; set; }
        }
    }

    public partial class Response
    {
        public class JournalLine
        {
            public Guid Id { get; set; }
            public Guid AccountId { get; set; }
            public string AccountCode { get; set; } = null!;
            public string AccountName { get; set; } = null!;
            public string? Description { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public int LineOrder { get; set; }
        }

        public class Single
        {
            public Guid Id { get; set; }
            public string EntryNumber { get; set; } = null!;
            public DateTime EntryDate { get; set; }
            public string Description { get; set; } = null!;
            public JournalEntryStatus Status { get; set; }
            public string StatusName => Status.ToString();
            public JournalEntryReferenceType ReferenceType { get; set; }
            public string ReferenceTypeName => ReferenceType.ToString();
            public Guid? ReferenceId { get; set; }
            public string? Reference { get; set; }
            public DateTime CreatedAt { get; set; }
            public decimal TotalDebit { get; set; }
            public decimal TotalCredit { get; set; }
            public List<JournalLine> Lines { get; set; } = new();
        }

        public class ListRow
        {
            public Guid Id { get; set; }
            public string EntryNumber { get; set; } = null!;
            public DateTime EntryDate { get; set; }
            public string Description { get; set; } = null!;
            public JournalEntryReferenceType ReferenceType { get; set; }
            public string ReferenceTypeName => ReferenceType.ToString();
            public string? Reference { get; set; }
            public decimal TotalAmount { get; set; }
            public int LineCount { get; set; }
            /// <summary>First non-zero-debit line — handy for "From / To" display in transfer rows.</summary>
            public string? FromAccount { get; set; }
            /// <summary>First non-zero-credit line — handy for "From / To" display in transfer rows.</summary>
            public string? ToAccount { get; set; }
        }

        public class Paged
        {
            public int Total { get; set; }
            public int TotalDisplay { get; set; }
            public List<ListRow> Data { get; set; } = new();
        }
    }
}
