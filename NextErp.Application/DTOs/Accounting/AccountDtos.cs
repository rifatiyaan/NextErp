using NextErp.Domain.Entities;

namespace NextErp.Application.DTOs.Accounting;

public partial class AccountDto
{
    public partial class Request
    {
        public class Create
        {
            public string Code { get; set; } = null!;
            public string Name { get; set; } = null!;
            public AccountType Type { get; set; }
            public Guid? ParentAccountId { get; set; }
            public bool IsPostingAllowed { get; set; } = true;
            public string? Description { get; set; }
        }

        public class Update
        {
            public string Name { get; set; } = null!;
            public AccountType Type { get; set; }
            public Guid? ParentAccountId { get; set; }
            public bool IsPostingAllowed { get; set; }
            public string? Description { get; set; }
            public bool IsActive { get; set; }
        }
    }

    public partial class Response
    {
        public class Single
        {
            public Guid Id { get; set; }
            public string Code { get; set; } = null!;
            public string Name { get; set; } = null!;
            public AccountType Type { get; set; }
            public string TypeName => Type.ToString();
            public Guid? ParentAccountId { get; set; }
            public string? ParentName { get; set; }
            public bool IsPostingAllowed { get; set; }
            public string? Description { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class Paged
        {
            public int Total { get; set; }
            public int TotalDisplay { get; set; }
            public List<Single> Data { get; set; } = new();
        }
    }
}
