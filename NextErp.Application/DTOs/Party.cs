using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs
{
    public partial class Party
    {
        public partial class Request
        {
            public abstract class Base
            {
                public string Title { get; set; } = null!;
                public string? FirstName { get; set; }
                public string? LastName { get; set; }
                public string? Email { get; set; }
                public string? Phone { get; set; }
                public string? Address { get; set; }
                public string? ContactPerson { get; set; }
                public string? LoyaltyCode { get; set; }
                public string? NationalId { get; set; }
                public string? VatNumber { get; set; }
                public string? TaxId { get; set; }
                public string? Notes { get; set; }
                public PartyType PartyType { get; set; }
            }

            public partial class Get
            {
                public class Single
                {
                    public Guid Id { get; set; }
                }

                public class Bulk
                {
                    public int Page { get; set; } = 1;
                    public int PageSize { get; set; } = 10;
                    public string? SearchTerm { get; set; }
                    public PartyType? PartyType { get; set; }
                    public bool? IsActive { get; set; }
                    public string? SortBy { get; set; }
                    public bool SortDescending { get; set; }
                }
            }

            public partial class Create
            {
                public class Single : Base
                {
                    public bool IsActive { get; set; } = true;
                }
            }

            public partial class Update
            {
                public class Single : Base
                {
                    public Guid Id { get; set; }
                    public bool IsActive { get; set; } = true;
                }
            }
        }

        public partial class Response
        {
            public abstract class Base
            {
                public Guid Id { get; set; }
                public string Title { get; set; } = null!;
                public string? FirstName { get; set; }
                public string? LastName { get; set; }
                public string? Email { get; set; }
                public string? Phone { get; set; }
                public string? Address { get; set; }
                public string? ContactPerson { get; set; }
                public string? LoyaltyCode { get; set; }
                public string? NationalId { get; set; }
                public string? VatNumber { get; set; }
                public string? TaxId { get; set; }
                public string? Notes { get; set; }
                public PartyType PartyType { get; set; }
                public bool IsActive { get; set; }
                public DateTime CreatedAt { get; set; }
                public DateTime? UpdatedAt { get; set; }
                public Guid TenantId { get; set; }
                public Guid BranchId { get; set; }
            }

            public partial class Get
            {
                public class Single : Base { }

                public class Bulk
                {
                    public List<Single> Records { get; set; } = new();
                    public int TotalCount { get; set; }
                    public int Page { get; set; }
                    public int PageSize { get; set; }
                    public int TotalPages { get; set; }
                }
            }

            public partial class Create
            {
                public class Single : Base { }
            }

            public partial class Update
            {
                public class Single : Base { }
            }
        }
    }
}
