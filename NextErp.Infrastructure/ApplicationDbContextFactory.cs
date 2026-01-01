using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NextErp.Infrastructure
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Simple connection string
            var connectionString = "Server=DESKTOP-2PTJ945\\SQLEXPRESS01;Database=NextErpTemp;Integrated Security=True;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString,
                x => x.MigrationsAssembly("NextErp.Infrastructure"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}