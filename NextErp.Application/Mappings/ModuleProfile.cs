using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class ModuleProfile : Profile
    {
        public ModuleProfile()
        {
            // ===== Request DTOs to Entity =====
            
            // Create Request -> Entity
            CreateMap<NextErp.Application.DTOs.Module.Request.Create.Single, NextErp.Domain.Entities.Module>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.InstalledAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore());

            // Hierarchical Create Request -> Entity (for bulk creation)
            CreateMap<NextErp.Application.DTOs.Module.Request.Create.Hierarchical, NextErp.Domain.Entities.Module>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.InstalledAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.ParentId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore());

            // Update Request -> Entity
            CreateMap<NextErp.Application.DTOs.Module.Request.Update.Single, NextErp.Domain.Entities.Module>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.InstalledAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore());

            // ===== Entity to Response DTOs =====
            
            // Entity -> Get Single Response
            CreateMap<NextErp.Domain.Entities.Module, NextErp.Application.DTOs.Module.Response.Get.Single>().MaxDepth(1);

            // Entity -> Create Single Response
            CreateMap<NextErp.Domain.Entities.Module, NextErp.Application.DTOs.Module.Response.Create.Single>();

            // Entity -> Create Hierarchical Response
            CreateMap<NextErp.Domain.Entities.Module, NextErp.Application.DTOs.Module.Response.Create.Hierarchical>().MaxDepth(1);

            // Entity -> Update Single Response
            CreateMap<NextErp.Domain.Entities.Module, NextErp.Application.DTOs.Module.Response.Update.Single>();

            // ===== Metadata Mappings =====
            
            CreateMap<NextErp.Domain.Entities.Module.ModuleMetadata, NextErp.Application.DTOs.Module.Request.Metadata>()
                .ReverseMap();
        }
    }
}
