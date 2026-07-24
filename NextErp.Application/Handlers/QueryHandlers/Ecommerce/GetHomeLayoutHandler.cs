using MediatR;
using NextErp.Application.Common.Settings;
using NextErp.Application.Queries.Ecommerce;
using NextErp.Application.Settings;

namespace NextErp.Application.Handlers.QueryHandlers.Ecommerce;

public class GetHomeLayoutHandler(ISettingsProvider settings)
    : IRequestHandler<GetHomeLayoutQuery, string>
{
    public async Task<string> Handle(GetHomeLayoutQuery request, CancellationToken cancellationToken = default)
    {
        var s = await settings.GetAsync<EcommerceSettings>(cancellationToken);
        return s.HomeLayoutJson ?? "";
    }
}
