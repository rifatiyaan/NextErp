using Autofac;
using Microsoft.AspNetCore.Http;
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

            // The UnitOfWork + per-entity Repository abstractions were removed; handlers now
            // depend on IApplicationDbContext directly. EF Core's DbContext already provides
            // a UnitOfWork (SaveChanges) and Repository (DbSet) implementation.

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
        }
    }
}
