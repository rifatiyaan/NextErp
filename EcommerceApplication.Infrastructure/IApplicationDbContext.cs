using EcommerceApplicationWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApplicationWeb.Infrastructure
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; set; }
        DbSet<Category> Categories { get; set; }
        //DbSet<User> Users { get; set; }
    }
}