using AutoMapper;
using Branch = NextErp.Domain.Entities.Branch;

namespace NextErp.Application.Mappings
{
    public class BranchProfile : Profile
    {
        public BranchProfile()
        {
            // ===== Request DTOs to Entity =====

            // Create Request -> Entity
            CreateMap<NextErp.Application.DTOs.Branch.Request.Create.Single, NextErp.Domain.Entities.Branch>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tenant, opt => opt.Ignore());

            // Update Request -> Entity
            CreateMap<NextErp.Application.DTOs.Branch.Request.Update.Single, NextErp.Domain.Entities.Branch>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tenant, opt => opt.Ignore());

            // ===== Entity to Response DTOs =====

            // Entity -> Get Single Response
            CreateMap<NextErp.Domain.Entities.Branch, NextErp.Application.DTOs.Branch.Response.Get.Single>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                .MaxDepth(1);

            // Entity -> Create Single Response
            CreateMap<NextErp.Domain.Entities.Branch, NextErp.Application.DTOs.Branch.Response.Create.Single>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title));

            // Entity -> Update Single Response
            CreateMap<NextErp.Domain.Entities.Branch, NextErp.Application.DTOs.Branch.Response.Update.Single>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title));

            // ===== Metadata Mappings =====

            CreateMap<NextErp.Domain.Entities.Branch.BranchMetadata, NextErp.Application.DTOs.Branch.Request.Metadata>()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
