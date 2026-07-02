using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.DTOs.SystemSettings;
using NextErp.Application.Interfaces;
using NextErp.Application.Mapping;
using NextErp.Application.Queries;
using DomainSystemSettings = NextErp.Domain.Entities.SystemSettings;

namespace NextErp.Application.Handlers.QueryHandlers.SystemSettings;

public class GetSystemSettingsHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetSystemSettingsQuery, SystemSettingsResponse>
{
    public async Task<SystemSettingsResponse> Handle(
        GetSystemSettingsQuery request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Empty;

        var existing = await dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        var entity = existing ?? DomainSystemSettings.CreateDefaults(tenantId);
        return entity.ToResponse();
    }
}
