namespace NextErp.Application.DTOs
{
    public partial class ProductVariation
    {
        public partial class Request
        {
            public class VariationOptionDto
            {
                public string Name { get; set; } = null!; // e.g., "Size", "Color"
                public int DisplayOrder { get; set; } = 0;
                public List<VariationValueDto> Values { get; set; } = new(); // e.g., ["S", "M", "L"] or ["Red", "Blue"]
            }

            public class VariationValueDto
            {
                public string Value { get; set; } = null!; // e.g., "S", "Red"
                public int DisplayOrder { get; set; } = 0;
            }

            public class ProductVariantDto
            {
                public string Sku { get; set; } = null!;
                public decimal Price { get; set; }
                public int Stock { get; set; }
                public bool IsActive { get; set; } = true;
                // Format: "optionIndex:valueIndex" - e.g., ["0:0", "1:1"] means first option's first value + second option's second value
                public List<string> VariationValueKeys { get; set; } = new(); 
            }
        }

        public partial class Response
        {
            public class VariationOptionDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = null!;
                public int DisplayOrder { get; set; }
                public List<VariationValueDto> Values { get; set; } = new();
            }

            public class VariationValueDto
            {
                public int Id { get; set; }
                public string Value { get; set; } = null!;
                public int DisplayOrder { get; set; }
            }

            public class ProductVariantDto
            {
                public int Id { get; set; }
                public string Sku { get; set; } = null!;
                public decimal Price { get; set; }
                public int Stock { get; set; }
                public bool IsActive { get; set; }
                public string Title { get; set; } = null!; // e.g., "S / Red"
                public List<VariationValueDto> VariationValues { get; set; } = new();
            }

            public class BulkVariationOptionDto
            {
                public string Name { get; set; } = null!;
                public List<string> Values { get; set; } = new();
            }
        }
    }
}

