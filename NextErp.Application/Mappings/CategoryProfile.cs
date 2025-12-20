using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // ===== Request DTOs to Entity =====
            
            // Create Request -> Entity
            CreateMap<NextErp.Application.DTOs.Category.Request.Create.Single, NextErp.Domain.Entities.Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // Update Request -> Entity
            CreateMap<NextErp.Application.DTOs.Category.Request.Update.Single, NextErp.Domain.Entities.Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // ===== Entity to Response DTOs =====
            
            // Entity -> Get Single Response
            CreateMap<NextErp.Domain.Entities.Category, NextErp.Application.DTOs.Category.Response.Get.Single>()
                .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.Products))
                .MaxDepth(1);

            // Entity -> Create Single Response
            CreateMap<NextErp.Domain.Entities.Category, NextErp.Application.DTOs.Category.Response.Create.Single>();

            // Entity -> Update Single Response
            CreateMap<NextErp.Domain.Entities.Category, NextErp.Application.DTOs.Category.Response.Update.Single>();

            // ===== Metadata Mappings =====
            
            CreateMap<NextErp.Domain.Entities.Category.CategoryMetadataClass, NextErp.Application.DTOs.Category.Request.Metadata>()
                .ReverseMap();

            // ===== Legacy Mappings (for backward compatibility during transition) =====
            
            // Request DTO -> Commands (keeping for existing handlers)
            CreateMap<NextErp.Application.DTOs.Category.Request.Create.Single, CreateCategoryCommand>()
                .ConstructUsing(dto => new CreateCategoryCommand(
                    dto.Title,
                    dto.Description,
                    dto.ParentId,
                    dto.IsActive
                ));

            CreateMap<NextErp.Application.DTOs.Category.Request.Update.Single, UpdateCategoryCommand>()
                .ConstructUsing(dto => new UpdateCategoryCommand(
                    dto.Id,
                    dto.Title,
                    dto.Description,
                    dto.ParentId,
                    dto.IsActive
                ));

            // Command -> Entity (for handlers)
            CreateMap<CreateCategoryCommand, NextErp.Domain.Entities.Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore());

            CreateMap<UpdateCategoryCommand, NextErp.Domain.Entities.Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore());
        }
    }
}
