namespace NextErp.Domain.Entities
{
    public static class SimpleProductVariantFactory
    {
        public static ProductVariant CreateDefault(Product product)
        {
            var code = string.IsNullOrWhiteSpace(product.Code) ? $"P{product.Id}" : product.Code.Trim();
            var sku = $"{code}-DEFAULT";

            return new ProductVariant
            {
                Title = "Default",
                Name = "Default",
                ProductId = product.Id,
                Sku = sku,
                Price = product.Price,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TenantId = product.TenantId,
                BranchId = product.BranchId
            };
        }
    }
}
