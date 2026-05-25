using Autofac;
using Microsoft.AspNetCore.Http;
using NextErp.Application.Common.Settings;
using NextErp.Application.Interfaces;
using NextErp.Application.Services;
using NextErp.Infrastructure.Services;

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

            builder.RegisterType<NotificationService>()
                .As<INotificationService>()
                .InstancePerLifetimeScope();

            // Discount + promotion rule engine. Wired in CreateSaleHandler
            // to auto-apply LineDiscount/Bogo/Membership/InvoiceDiscount.
            builder.RegisterType<PricingService>()
                .As<IPricingService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<NextErp.Infrastructure.Services.SettingsProvider>()
                .As<ISettingsProvider>()
                .InstancePerLifetimeScope();

            // PDF rendering services live in Application/Services because the
            // generators only depend on DTOs — registered here as Infrastructure
            // owns the DI module. Keeping the contract consistent with the
            // other services and avoids any surprise if we later inject a
            // tenant-scoped branding profile.
            builder.RegisterType<InvoicePdfService>()
                .As<IInvoicePdfService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<PurchaseInvoicePdfService>()
                .As<IPurchaseInvoicePdfService>()
                .InstancePerLifetimeScope();

            builder.RegisterType<ReportPdfService>()
                .As<IReportPdfService>()
                .InstancePerLifetimeScope();

            // SMTP-backed email sender. Resolved both from the controller
            // (when enqueueing a Hangfire job by interface) and inside the
            // Hangfire worker (when the job activator instantiates it).
            // Lifetime scope keeps EF + per-request services consistent.
            builder.RegisterType<Services.SmtpInvoiceEmailService>()
                .As<IInvoiceEmailService>()
                .InstancePerLifetimeScope();

            // Broadcast (no-PDF) customer email sender. Shares the same
            // EmailOptions/SMTP relay as the invoice service — see
            // SmtpCustomerBroadcastEmailService for the rationale.
            builder.RegisterType<Services.SmtpCustomerBroadcastEmailService>()
                .As<ICustomerBroadcastEmailService>()
                .InstancePerLifetimeScope();
        }
    }
}
