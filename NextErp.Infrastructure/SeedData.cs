using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure.Seeds
{
    public static class SeedData
    {
        public static void SeedCategories(ModelBuilder modelBuilder)
        {
            // Categories Seed
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Title = "Electronics", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "All electronic devices", Metadata = new Category.CategoryMetadataClass { ProductCount = "50", Department = "Tech" } },
                new Category { Id = 2, Title = "Clothing", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Fashion and apparel", Metadata = new Category.CategoryMetadataClass { ProductCount = "120", Department = "Fashion" } },
                new Category { Id = 3, Title = "Books", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "All kinds of books", Metadata = new Category.CategoryMetadataClass { ProductCount = "200", Department = "Literature" } },
                new Category { Id = 4, Title = "Home Appliances", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Appliances for home", Metadata = new Category.CategoryMetadataClass { ProductCount = "80", Department = "Home" } },
                new Category { Id = 5, Title = "Sports", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Sports equipment", Metadata = new Category.CategoryMetadataClass { ProductCount = "60", Department = "Sports" } },
                new Category { Id = 6, Title = "Toys", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Toys for children", Metadata = new Category.CategoryMetadataClass { ProductCount = "100", Department = "Kids" } },
                new Category { Id = 7, Title = "Beauty", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Beauty and personal care", Metadata = new Category.CategoryMetadataClass { ProductCount = "70", Department = "Cosmetics" } },
                new Category { Id = 8, Title = "Automotive", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Car and bike accessories", Metadata = new Category.CategoryMetadataClass { ProductCount = "90", Department = "Automotive" } },
                new Category { Id = 9, Title = "Grocery", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Daily groceries", Metadata = new Category.CategoryMetadataClass { ProductCount = "150", Department = "Food" } },
                new Category { Id = 10, Title = "Furniture", ParentId = null, CreatedAt = DateTime.UtcNow, Description = "Home and office furniture", Metadata = new Category.CategoryMetadataClass { ProductCount = "40", Department = "Home" } }
            );
        }
        public static void SeedProducts(ModelBuilder modelBuilder)
        {
            // Products Seed
            modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Title = "iPhone 15", Code = "IP15-001", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 1299.99m, Stock = 10, CategoryId = 1, ImageUrl = "https://example.com/iphone15.jpg", Metadata = new Product.ProductMetadataClass { Description = "Latest iPhone", Color = "Black", Warranty = "2 Years" } },
            new Product { Id = 2, Title = "Samsung TV", Code = "TV-002", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 899.50m, Stock = 15, CategoryId = 1, ImageUrl = "https://example.com/samsungtv.jpg", Metadata = new Product.ProductMetadataClass { Description = "Smart LED TV", Color = "Silver", Warranty = "3 Years" } },
            new Product { Id = 3, Title = "Men's Jacket", Code = "MJKT-003", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 199.50m, Stock = 25, CategoryId = 2, ImageUrl = "https://example.com/jacket.jpg", Metadata = new Product.ProductMetadataClass { Description = "Warm winter jacket", Color = "Brown", Warranty = "1 Year" } },
            new Product { Id = 4, Title = "Novel: The Alchemist", Code = "BK-004", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 12.99m, Stock = 100, CategoryId = 3, ImageUrl = "https://example.com/alchemist.jpg", Metadata = new Product.ProductMetadataClass { Description = "Fiction book", Color = null, Warranty = null } },
            new Product { Id = 5, Title = "Microwave Oven", Code = "MO-005", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 299.00m, Stock = 20, CategoryId = 4, ImageUrl = "https://example.com/microwave.jpg", Metadata = new Product.ProductMetadataClass { Description = "800W Microwave", Color = "White", Warranty = "2 Years" } },
            new Product { Id = 6, Title = "Football", Code = "SP-006", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 29.99m, Stock = 50, CategoryId = 5, ImageUrl = "https://example.com/football.jpg", Metadata = new Product.ProductMetadataClass { Description = "Official size 5 football", Color = "White/Black", Warranty = null } },
            new Product { Id = 7, Title = "Lego Set", Code = "TOY-007", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 59.99m, Stock = 30, CategoryId = 6, ImageUrl = "https://example.com/lego.jpg", Metadata = new Product.ProductMetadataClass { Description = "Creative Lego set", Color = "Multi-color", Warranty = null } },
            new Product { Id = 8, Title = "Lipstick Set", Code = "BTY-008", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 49.99m, Stock = 40, CategoryId = 7, ImageUrl = "https://example.com/lipstick.jpg", Metadata = new Product.ProductMetadataClass { Description = "Matte lipstick", Color = "Red", Warranty = null } },
            new Product { Id = 9, Title = "Car Vacuum Cleaner", Code = "AUTO-009", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 79.99m, Stock = 18, CategoryId = 8, ImageUrl = "https://example.com/carvac.jpg", Metadata = new Product.ProductMetadataClass { Description = "Portable vacuum for car", Color = "Black", Warranty = "6 Months" } },
            new Product { Id = 10, Title = "Office Chair", Code = "FUR-010", ParentId = null, CreatedAt = DateTime.UtcNow, Price = 149.99m, Stock = 12, CategoryId = 10, ImageUrl = "https://example.com/chair.jpg", Metadata = new Product.ProductMetadataClass { Description = "Ergonomic chair", Color = "Black", Warranty = "1 Year" } }
        );
        }

    }
}

