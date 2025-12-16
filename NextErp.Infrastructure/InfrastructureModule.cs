using Autofac;
using Microsoft.EntityFrameworkCore;

namespace NextErp.Infrastructure
{
    public class InfrastructureModule : Module
    {
        private readonly string _connectionString;
        private readonly string _migrationAssembly;
        private readonly string _databaseProvider;

        public InfrastructureModule(string connectionString, string migrationAssembly, string databaseProvider = "SqlServer")
        {
            _connectionString = connectionString;
            _migrationAssembly = migrationAssembly;
            _databaseProvider = databaseProvider;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Register DbContext
            builder.Register(context =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

                if (_databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
                {
                    optionsBuilder.UseNpgsql(_connectionString,
                        npgsqlOptions => npgsqlOptions.MigrationsAssembly(_migrationAssembly));
                }
                else
                {
                    optionsBuilder.UseSqlServer(_connectionString,
                        sqlOptions => sqlOptions.MigrationsAssembly(_migrationAssembly));
                }

                return new ApplicationDbContext(optionsBuilder.Options);
            })
            .As<IApplicationDbContext>()
            .AsSelf()
            .InstancePerLifetimeScope();

            // Register other infrastructure services here
            builder.RegisterType<ApplicationUnitOfWork>()
                .As<IApplicationUnitOfWork>()
                .InstancePerLifetimeScope();
            
            // Register Repositories
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}