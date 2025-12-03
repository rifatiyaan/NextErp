using Autofac;
using EcommerceApplicationWeb.Application;
using EcommerceApplicationWeb.Domain.Repositories;
using EcommerceApplicationWeb.Infrastructure.Repositories;

namespace EcommerceApplicationWeb.Infrastructure
{
    public class InfrastructureModule : Module
    {
        private readonly string _connectionString;
        private readonly string _migrationAssembly;

        public InfrastructureModule(string connectionString, string migrationAssembly)
        {
            _connectionString = connectionString;
            _migrationAssembly = migrationAssembly;
        }
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ApplicationDbContext>().AsSelf()
                .WithParameter("connectionString", _connectionString)
                .WithParameter("migrationAssembly", _migrationAssembly)
                .InstancePerLifetimeScope();

            builder.RegisterType<ApplicationDbContext>().As<IApplicationDbContext>()
               .WithParameter("connectionString", _connectionString)
               .WithParameter("migrationAssembly", _migrationAssembly)
               .InstancePerLifetimeScope();

            builder.RegisterType<ApplicationUnitOfWork>().As<IApplicationUnitOfWork>().InstancePerLifetimeScope();
            //builder.RegisterType<BookRepository>().As<IBookRepository>().InstancePerLifetimeScope();

            builder.RegisterType<CategoryRepository>().As<ICategoryRepository>().InstancePerLifetimeScope();
            builder.RegisterType<UserRepository>().As<IUserRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ProductRepository>().As<IProductRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MenuItemRepository>().As<IMenuItemRepository>().InstancePerLifetimeScope();
            builder.RegisterType<ModuleRepository>().As<IModuleRepository>().InstancePerLifetimeScope();
        }
    }
}
