namespace NextErp.Application.DTOs;

public partial class UnitOfMeasure
{
    public partial class Request
    {
        public class Create
        {
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
        }

        public class Update
        {
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
            public bool IsActive { get; set; }
        }
    }

    public partial class Response
    {
        public class Single
        {
            public int Id { get; set; }
            public string Name { get; set; } = null!;
            public string Abbreviation { get; set; } = null!;
            public bool IsActive { get; set; }
        }
    }
}
