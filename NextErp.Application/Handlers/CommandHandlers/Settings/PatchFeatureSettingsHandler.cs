using MediatR;
using NextErp.Application.Commands.Settings;
using NextErp.Application.Common.Settings;

namespace NextErp.Application.Handlers.CommandHandlers.Settings;

public class PatchFeatureSettingsHandler(ISettingsProvider provider)
    : IRequestHandler<PatchFeatureSettingsCommand, Unit>
{
    public async Task<Unit> Handle(PatchFeatureSettingsCommand request, CancellationToken cancellationToken = default)
    {
        await provider.PatchAsync(request.Module, request.Values, cancellationToken);
        return Unit.Value;
    }
}
