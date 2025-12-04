using NextErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure
{
    public interface IApplicationDbContext
    {
        DbSet<Product> Products { get; set; }
        DbSet<Category> Categories { get; set; }
        //DbSet<User> Users { get; set; }
    }
}