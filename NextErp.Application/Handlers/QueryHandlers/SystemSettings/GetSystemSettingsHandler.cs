using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NextErp.Application.Interfaces;
using NextErp.Application.Queries;
using DomainSystemSettings = NextErp.Domain.Entities.SystemSettings;
using SystemSettingsDto = NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Handlers.QueryHandlers.SystemSettings;

public class GetSystemSettingsHandler(IApplicationDbContext dbContext, IMapper mapper)
    : IRequestHandler<GetSystemSettingsQuery, SystemSettingsDto.Response.Single>
{
    public async Task<SystemSettingsDto.Response.Single> Handle(
        GetSystemSettingsQuery request,
        CancellationToken cancellationToken = default)
    {
        var tenantId = Guid.Empty;

        var existing = await dbContext.SystemSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

        var entity = existing ?? DomainSystemSettings.CreateDefaults(tenantId);
        return mapper.Map<SystemSettingsDto.Response.Single>(entity);
    }
}

