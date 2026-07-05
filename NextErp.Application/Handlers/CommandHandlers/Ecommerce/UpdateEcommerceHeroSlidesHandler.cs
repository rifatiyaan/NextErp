using MediatR;
using NextErp.Application.Commands.Ecommerce;
using NextErp.Application.Common.Settings;
using NextErp.Application.Handlers.QueryHandlers.Ecommerce;
using NextErp.Application.Settings;

namespace NextErp.Application.Handlers.CommandHandlers.Ecommerce;

public class UpdateEcommerceHeroSlidesHandler(ISettingsProvider settings)
    : IRequestHandler<UpdateEcommerceHeroSlidesCommand, int>
{
    public async Task<int> Handle(UpdateEcommerceHeroSlidesCommand request, CancellationToken cancellationToken = default)
    {
        var current = await settings.GetAsync<EcommerceSettings>(cancellationToken);
        var json = StoreQueryShared.SerializeSlides(request.Slides);
        current.HeroSlidesJson = json;
        await settings.UpdateAsync(current, cancellationToken);

        // Report how many valid slides were actually stored (blank-image dropped).
        return StoreQueryShared.ParseSlides(json).Count;
    }
}
