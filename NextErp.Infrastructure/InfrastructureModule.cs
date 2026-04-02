using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NextErp.Application;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;

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

                var branchProvider = context.ResolveOptional<IBranchProvider>();
                return new ApplicationDbContext(optionsBuilder.Options, branchProvider);
            })
            .As<IApplicationDbContext>()
            .AsSelf()
            .InstancePerLifetimeScope();

            // Register other infrastructure services here
            builder.RegisterType<ApplicationUnitOfWork>()
                .As<IApplicationUnitOfWork>()
                .InstancePerLifetimeScope();

            builder.RegisterType<Services.CloudinaryService>()
                .As<IImageService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterType<Services.BranchProvider>()
                .As<IBranchProvider>()
                .InstancePerLifetimeScope();

            builder.RegisterType<StockService>()
                .As<IStockService>()
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