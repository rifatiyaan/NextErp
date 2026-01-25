using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NextErp.Infrastructure
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Connection string for Docker SQL Server (matches appsettings.json)
            // Use 1434 to avoid clashing with any local/office SQL Server bound to 1433
            var connectionString = "Server=localhost,1434;Database=MyDb;User Id=sa;Password=StrongPassword123!;TrustServerCertificate=True;";

            optionsBuilder.UseSqlServer(connectionString,
                x => x.MigrationsAssembly("NextErp.API"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}