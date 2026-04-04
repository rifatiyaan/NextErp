using Autofac;
using Microsoft.AspNetCore.Http;
using NextErp.Application;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;

namespace NextErp.Infrastructure
{
    public class InfrastructureModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // ApplicationDbContext is registered via builder.Services.AddDbContext in Program.cs
            // (single registration; IBranchProvider is injected by DI when resolving the context).

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