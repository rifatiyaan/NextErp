using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.SystemSettings;
using NextErp.Application.Interfaces;
using DomainSystemSettings = NextErp.Domain.Entities.SystemSettings;
using SystemSettingsDto = NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Handlers.CommandHandlers.SystemSettings;

public class UpdateSystemSettingsHandler(IApplicationDbContext dbContext, IMapper mapper)
    : IRequestHandler<UpdateSystemSettingsCommand, SystemSettingsDto.Response.Single>
{
    public async Task<SystemSettingsDto.Response.Single> Handle(
        UpdateSystemSettingsCommand request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Empty;

        var entity = await dbContext.SystemSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        if (entity == null)
        {
            entity = DomainSystemSettings.CreateDefaults(tenantId);
            dbContext.SystemSettings.Add(entity);
        }

        mapper.Map(request.Dto, entity);

        // If the caller explicitly switched to a preset, clear stale custom values
        // (and vice versa). The validator rejects mixed payloads up-front; this is
        // a belt-and-suspenders for partial-update edge cases.
        if (!string.IsNullOrWhiteSpace(request.Dto.PresetAccentTheme))
        {
            entity.CustomPrimary = null;
            entity.CustomSecondary = null;
            entity.CustomSidebarBackground = null;
            entity.CustomSidebarForeground = null;
        }
        else if (HasAnyCustomColor(request.Dto))
        {
            entity.PresetAccentTheme = null;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return mapper.Map<SystemSettingsDto.Response.Single>(entity);
    }

    private static bool HasAnyCustomColor(SystemSettingsDto.Request.Update dto) =>
        !string.IsNullOrWhiteSpace(dto.CustomPrimary) ||
        !string.IsNullOrWhiteSpace(dto.CustomSecondary) ||
        !string.IsNullOrWhiteSpace(dto.CustomSidebarBackground) ||
        !string.IsNullOrWhiteSpace(dto.CustomSidebarForeground);
}

