using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NextErp.Application.Common.Settings;
using NextErp.Application.Settings;

namespace NextErp.API.Filters;

// All public store endpoints 403 while the storefront is switched off. The
// frontend maps 403 to its designed "store closed" page.
public class StorefrontEnabledFilter(ISettingsProvider settings) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var ecommerce = await settings.GetAsync<EcommerceSettings>();
        if (!ecommerce.StorefrontEnabled)
        {
            context.Result = new ObjectResult(new { message = "The store is currently closed." })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
            return;
        }
        await next();
    }
}
