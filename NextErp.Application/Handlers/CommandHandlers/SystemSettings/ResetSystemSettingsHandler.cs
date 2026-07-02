using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.SystemSettings;
using NextErp.Application.DTOs.SystemSettings;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using DomainSystemSettings = NextErp.Domain.Entities.SystemSettings;

namespace NextErp.Application.Handlers.CommandHandlers.SystemSettings;

public class ResetSystemSettingsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<ResetSystemSettingsCommand, SystemSettingsResponse>
{
    public async Task<SystemSettingsResponse> Handle(
        ResetSystemSettingsCommand request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Empty;

        var entity = await dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        var defaults = DomainSystemSettings.CreateDefaults(tenantId);

        if (entity == null)
        {
            dbContext.SystemSettings.Add(defaults);
            await dbContext.SaveChangesAsync(cancellationToken);
            return defaults.ToResponse();
        }

        entity.PresetAccentTheme = defaults.PresetAccentTheme;
        entity.CustomPrimary = null;
        entity.CustomSecondary = null;
        entity.CustomSidebarBackground = null;
        entity.CustomSidebarForeground = null;
        entity.NavigationPlacement = defaults.NavigationPlacement;
        entity.Radius = defaults.Radius;
        entity.CompanyName = null;
        entity.CompanyLogoUrl = null;
        entity.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToResponse();
    }
}
