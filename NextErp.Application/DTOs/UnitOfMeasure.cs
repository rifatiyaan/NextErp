namespace NextErp.Application.DTOs;

public partial class UnitOfMeasure
{
    public partial class Request
    {
        public class Create
        {
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
            public string? Category { get; set; }
            public bool IsSystem { get; set; } = false;
        }

        public class Update
        {
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
            public string? Category { get; set; }
            public bool IsActive { get; set; }
        }
    }

    public partial class Response
    {
        public class Single
        {
            public int Id { get; set; }
            public string Title { get; set; } = null!;
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
            public string? Category { get; set; }
            public bool IsSystem { get; set; }
            public bool IsActive { get; set; }
        }
    }
}
