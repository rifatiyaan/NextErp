using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.SystemSettings;
using NextErp.Application.Interfaces;
using DomainSystemSettings = NextErp.Domain.Entities.SystemSettings;
using SystemSettingsDto = NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Handlers.CommandHandlers.SystemSettings;

public class ResetSystemSettingsHandler(IApplicationDbContext dbContext, IMapper mapper)
    : IRequestHandler<ResetSystemSettingsCommand, SystemSettingsDto.Response.Single>
{
    public async Task<SystemSettingsDto.Response.Single> Handle(
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
            return mapper.Map<SystemSettingsDto.Response.Single>(defaults);
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
        return mapper.Map<SystemSettingsDto.Response.Single>(entity);
    }
}

