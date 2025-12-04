using AutoMapper;
using EcommerceApplicationWeb.Application.Commands;
using EcommerceApplicationWeb.Application.DTOs;
using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // Entity <-> Response DTO
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ReverseMap();

            // Request DTO -> Entity
            CreateMap<ProductRequestDto, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId ?? 0));

            // Metadata mappings
            CreateMap<Product.ProductMetadataClass, ProductMetadataDto>()
                .ReverseMap();

            // Request DTO -> Commands
            CreateMap<ProductRequestDto, CreateProductCommand>()
                .ConstructUsing(dto => new CreateProductCommand(
                    dto.Title,
                    dto.Code,
                    dto.ParentId,
                    dto.Metadata != null && dto.Metadata.CategoryId.HasValue ? dto.Metadata.CategoryId.Value : (dto.CategoryId ?? 0),
                    dto.Price,
                    dto.Stock,
                    dto.ImageUrl,
                    dto.Metadata != null ? dto.Metadata.Description : null,
                    dto.Metadata != null ? dto.Metadata.Color : null,
                    dto.Metadata != null ? dto.Metadata.Warranty : null
                ));

            CreateMap<ProductRequestDto, UpdateProductCommand>()
                .ConstructUsing((dto, ctx) =>
                {
                    var id = ctx.Items.ContainsKey("Id") ? (int)ctx.Items["Id"] : 0;
                    return new UpdateProductCommand(
                        id,
                        dto.Title,
                        dto.Code,
                        dto.ParentId,
                        dto.Metadata != null && dto.Metadata.CategoryId.HasValue ? dto.Metadata.CategoryId.Value : (dto.CategoryId ?? 0),
                        dto.Price,
                        dto.Stock,
                        dto.ImageUrl,
                        dto.Metadata != null ? dto.Metadata.Description : null,
                        dto.Metadata != null ? dto.Metadata.Color : null,
                        dto.Metadata != null ? dto.Metadata.Warranty : null
                    );
                });

            // Command -> Entity (for handlers)
            CreateMap<CreateProductCommand, Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new Product.ProductMetadataClass
                {
                    Description = src.Description,
                    Color = src.Color,
                    Warranty = src.Warranty
                }));

            CreateMap<UpdateProductCommand, Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new Product.ProductMetadataClass
                {
                    Description = src.Description,
                    Color = src.Color,
                    Warranty = src.Warranty
                }));
        }
    }
}
