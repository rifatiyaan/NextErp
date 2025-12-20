using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class TenantProfile : Profile
    {
        public TenantProfile()
        {
            // ===== Request DTOs to Entity =====
            
            // Create Request -> Entity
            CreateMap<NextErp.Application.DTOs.Tenant.Request.Create.Single, NextErp.Domain.Entities.Tenant>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Branches, opt => opt.Ignore());

            // Update Request -> Entity
            CreateMap<NextErp.Application.DTOs.Tenant.Request.Update.Single, NextErp.Domain.Entities.Tenant>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Branches, opt => opt.Ignore());

            // ===== Entity to Response DTOs =====
            
            // Entity -> Get Single Response
            CreateMap<NextErp.Domain.Entities.Tenant, NextErp.Application.DTOs.Tenant.Response.Get.Single>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                .MaxDepth(1);

            // Entity -> Create Single Response
            CreateMap<NextErp.Domain.Entities.Tenant, NextErp.Application.DTOs.Tenant.Response.Create.Single>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title));

            // Entity -> Update Single Response
            CreateMap<NextErp.Domain.Entities.Tenant, NextErp.Application.DTOs.Tenant.Response.Update.Single>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title));

            // ===== Metadata Mappings =====
            
            CreateMap<NextErp.Domain.Entities.Tenant.TenantMetadata, NextErp.Application.DTOs.Tenant.Request.Metadata>()
                .ReverseMap();
        }
    }
}
