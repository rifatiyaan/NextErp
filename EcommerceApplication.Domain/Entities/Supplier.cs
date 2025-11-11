namespace EcommerceApplicationWeb.Domain.Entities
{
    public class Supplier : IEntity<int>
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;

        public string? ContactPerson { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public SupplierMetadata Metadata { get; set; } = new SupplierMetadata();

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Guid TenantId { get; set; }
        public Guid? BranchId { get; set; }

        public ICollection<PurchaseInvoice> PurchaseInvoices { get; set; } = new List<PurchaseInvoice>();

        public class SupplierMetadata
        {
            public string? VatNumber { get; set; }
            public string? TaxId { get; set; }
            public string? Notes { get; set; }
        }
    }

}
