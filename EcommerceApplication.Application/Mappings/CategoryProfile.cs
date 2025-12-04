using AutoMapper;
using EcommerceApplicationWeb.Application.Commands;
using EcommerceApplicationWeb.Application.DTOs;
using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            // Entity <-> Response DTO
            CreateMap<Category, CategoryResponseDto>()
                .ReverseMap();

            // Request DTO -> Entity
            CreateMap<CategoryRequestDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore());

            // Metadata mappings
            CreateMap<Category.CategoryMetadataClass, CategoryMetadataDto>()
                .ReverseMap();

            // Request DTO -> Commands
            CreateMap<CategoryRequestDto, CreateCategoryCommand>()
                .ConstructUsing(dto => new CreateCategoryCommand(
                    dto.Title,
                    dto.Description,
                    dto.ParentId
                ));

            CreateMap<CategoryRequestDto, UpdateCategoryCommand>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ConstructUsing((dto, ctx) =>
                {
                    var id = ctx.Items.ContainsKey("Id") ? (int)ctx.Items["Id"] : 0;
                    return new UpdateCategoryCommand(
                        id,
                        dto.Title,
                        dto.Description,
                        dto.ParentId
                    );
                });

            // Command -> Entity (for handlers)
            CreateMap<CreateCategoryCommand, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<UpdateCategoryCommand, Category>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Products, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());
        }
    }
}
