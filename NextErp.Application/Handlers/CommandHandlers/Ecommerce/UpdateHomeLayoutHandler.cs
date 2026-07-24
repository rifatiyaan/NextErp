using System.Text.Json;
using MediatR;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Common.Settings;
using NextErp.Application.Settings;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class UpdateHomeLayoutHandler(ISettingsProvider settings)
    : IRequestHandler<UpdateHomeLayoutCommand, int>
{
    public async Task<int> Handle(UpdateHomeLayoutCommand request, CancellationToken cancellationToken = default)
    {
        // The section schema lives on the frontend; the backend is a dumb JSON
        // store. Validate only that it's a JSON array — anything else is stored
        // as empty so the storefront falls back to its built-in default layout.
        var json = (request.LayoutJson ?? "").Trim();
        var count = 0;
        if (json.Length > 0)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    count = doc.RootElement.GetArrayLength();
                else
                    json = "";
            }
            catch (JsonException)
            {
                json = "";
            }
        }

        var current = await settings.GetAsync<EcommerceSettings>(cancellationToken);
        current.HomeLayoutJson = json;
        await settings.UpdateAsync(current, cancellationToken);
        return count;
    }
}
