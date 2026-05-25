using MediatR;
using NextErp.Application.Common.Settings;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.Settings;

public class GetFeatureSettingsSchemaHandler(ISettingsProvider provider)
    : IRequestHandler<GetFeatureSettingsSchemaQuery, SettingsSchema>
{
    public Task<SettingsSchema> Handle(GetFeatureSettingsSchemaQuery request, CancellationToken cancellationToken = default)
        => Task.FromResult(provider.GetSchema());
}
