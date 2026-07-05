namespace NextErp.Domain.Entities
{
    // NOT [BranchScoped]: public product reviews are global to the product row.
    public class Review : IEntity<int>
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string AuthorName { get; set; } = null!;
        public int Rating { get; set; } // 1..5
        public string Text { get; set; } = null!;

        // Auto-approved for the COD MVP; the flag lets staff hide a review later.
        public bool IsApproved { get; set; } = true;

        public Guid TenantId { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// IEntity contract requirement. Mirrors <see cref="AuthorName"/> —
        /// reviews carry no human-meaningful title of their own.
        /// </summary>
        public string Title
        {
            get => AuthorName;
            set => AuthorName = value;
        }
    }
}
