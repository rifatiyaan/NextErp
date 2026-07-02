using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Commands.SystemSettings;
using NextErp.Application.DTOs.SystemSettings;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using DomainSystemSettings = NextErp.Domain.Entities.SystemSettings;

namespace NextErp.Application.Handlers.CommandHandlers.SystemSettings;

public class UpdateSystemSettingsHandler(
    IApplicationDbContext dbContext,
    INotificationService notifications)
    : IRequestHandler<UpdateSystemSettingsCommand, SystemSettingsResponse>
{
    public async Task<SystemSettingsResponse> Handle(
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

        request.Dto.ApplyTo(entity);

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

        await notifications.RecordAsync(
            type: "SystemSettingsUpdated",
            title: "Appearance updated",
            message: "Theme/branding changed",
            relatedEntityType: "SystemSettings",
            relatedEntityId: entity.Id.ToString(),
            cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToResponse();
    }

    private static bool HasAnyCustomColor(UpdateSystemSettingsRequest dto) =>
        !string.IsNullOrWhiteSpace(dto.CustomPrimary) ||
        !string.IsNullOrWhiteSpace(dto.CustomSecondary) ||
        !string.IsNullOrWhiteSpace(dto.CustomSidebarBackground) ||
        !string.IsNullOrWhiteSpace(dto.CustomSidebarForeground);
}
