using AutoMapper;
using NextErp.Domain.Entities;
using SystemSettingsDto = NextErp.Application.DTOs.SystemSettings;

namespace NextErp.Application.Mappings;

public class SystemSettingsProfile : Profile
{
    public SystemSettingsProfile()
    {
        CreateMap<SystemSettings, SystemSettingsDto.Response.Single>();

        CreateMap<SystemSettingsDto.Request.Update, SystemSettings>()
            // Don't overwrite identity / audit / scope columns from the request
            .ForMember(d => d.Id, o => o.Ignore())
            .ForMember(d => d.TenantId, o => o.Ignore())
            .ForMember(d => d.Title, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            // Only overwrite when the request explicitly provides a value (Condition).
            // Null in the request means "leave existing value alone" so partial updates
            // don't accidentally clear fields the caller didn't intend to touch.
            .ForAllMembers(o => o.Condition((src, dest, srcMember) => srcMember != null));
    }
}
