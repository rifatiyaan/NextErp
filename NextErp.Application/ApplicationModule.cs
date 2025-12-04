using Autofac;
using MediatR;

namespace NextErp.Application
{
    public class ApplicationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Mediator>()
              .As<IMediator>()
              .InstancePerLifetimeScope();

            // ? Auto-register all Handlers from this Assembly
            builder.RegisterAssemblyTypes(typeof(ApplicationAssemblyMarker).Assembly)
                .AsClosedTypesOf(typeof(IRequestHandler<,>))
                .InstancePerLifetimeScope();
        }
    }
}
