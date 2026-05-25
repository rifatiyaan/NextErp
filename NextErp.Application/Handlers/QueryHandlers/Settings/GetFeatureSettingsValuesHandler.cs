using MediatR;
using NextErp.Application.Common.Settings;
using NextErp.Application.Queries;

namespace NextErp.Application.Handlers.QueryHandlers.Settings;

public class GetFeatureSettingsValuesHandler(ISettingsProvider provider)
    : IRequestHandler<GetFeatureSettingsValuesQuery, IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>>
{
    public Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, object?>>> Handle(
        GetFeatureSettingsValuesQuery request,
        CancellationToken cancellationToken = default)
        => provider.GetAllValuesAsync(cancellationToken);
}
