using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            // ===== Request DTOs to Entity =====
            
            // Create Request -> Entity
            CreateMap<NextErp.Application.DTOs.Product.Request.Create.Single, NextErp.Domain.Entities.Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Variations, opt => opt.Ignore())
                .ForMember(dest => dest.VariationOptions, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
                .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId ?? 0));

            // Update Request -> Entity
            CreateMap<NextErp.Application.DTOs.Product.Request.Update.Single, NextErp.Domain.Entities.Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Variations, opt => opt.Ignore())
                .ForMember(dest => dest.VariationOptions, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
                .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId ?? 0));

            // ===== Entity to Response DTOs =====
            
            // Entity -> Get Single Response
            CreateMap<NextErp.Domain.Entities.Product, NextErp.Application.DTOs.Product.Response.Get.Single>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(dest => dest.HasVariations, opt => opt.MapFrom(src => 
                    src.HasVariations || (src.VariationOptions != null && src.VariationOptions.Any())))
                .ForMember(dest => dest.VariationOptions, opt => opt.MapFrom(src => 
                    (src.VariationOptions != null && src.VariationOptions.Any()) 
                        ? src.VariationOptions.OrderBy(vo => vo.DisplayOrder).ToList() 
                        : null))
                .ForMember(dest => dest.ProductVariants, opt => opt.MapFrom(src => 
                    (src.ProductVariants != null && src.ProductVariants.Any()) 
                        ? src.ProductVariants.ToList() 
                        : null))
                .MaxDepth(3);

            // Entity -> Create Single Response
            CreateMap<NextErp.Domain.Entities.Product, NextErp.Application.DTOs.Product.Response.Create.Single>()
                .MaxDepth(1);

            // Entity -> Update Single Response
            CreateMap<NextErp.Domain.Entities.Product, NextErp.Application.DTOs.Product.Response.Update.Single>()
                .MaxDepth(1);

            // ===== Metadata Mappings =====
            
            CreateMap<NextErp.Domain.Entities.Product.ProductMetadataClass, NextErp.Application.DTOs.Product.Request.Metadata>()
                .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
                .ReverseMap();

            // ===== Legacy Mappings (for backward compatibility during transition) =====
            
            // Request DTO -> Commands (keeping for existing handlers)
            CreateMap<NextErp.Application.DTOs.Product.Request.Create.Single, CreateProductCommand>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Metadata.Description))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Metadata.Color))
                .ForMember(dest => dest.Warranty, opt => opt.MapFrom(src => src.Metadata.Warranty))
                .ConstructUsing(dto => new CreateProductCommand(
                    dto.Title,
                    dto.Code,
                    dto.ParentId,
                    dto.Metadata != null && dto.Metadata.CategoryId.HasValue ? dto.Metadata.CategoryId.Value : (dto.CategoryId ?? 0),
                    dto.Price,
                    dto.Stock,
                    dto.IsActive,
                    dto.ImageUrl,
                    dto.Metadata != null ? dto.Metadata.Description : null,
                    dto.Metadata != null ? dto.Metadata.Color : null,
                    dto.Metadata != null ? dto.Metadata.Warranty : null
                ));

            CreateMap<NextErp.Application.DTOs.Product.Request.Update.Single, UpdateProductCommand>()
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Metadata.Description))
                .ForMember(dest => dest.Color, opt => opt.MapFrom(src => src.Metadata.Color))
                .ForMember(dest => dest.Warranty, opt => opt.MapFrom(src => src.Metadata.Warranty))
                .ConstructUsing(dto => new UpdateProductCommand(
                    dto.Id,
                    dto.Title,
                    dto.Code,
                    dto.ParentId,
                    dto.Metadata != null && dto.Metadata.CategoryId.HasValue ? dto.Metadata.CategoryId.Value : (dto.CategoryId ?? 0),
                    dto.Price,
                    dto.Stock,
                    dto.IsActive,
                    dto.ImageUrl,
                    dto.Metadata != null ? dto.Metadata.Description : null,
                    dto.Metadata != null ? dto.Metadata.Color : null,
                    dto.Metadata != null ? dto.Metadata.Warranty : null
                ));

            // Command -> Entity (for handlers)
            CreateMap<CreateProductCommand, NextErp.Domain.Entities.Product>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Variations, opt => opt.Ignore())
                .ForMember(dest => dest.VariationOptions, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
                .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new NextErp.Domain.Entities.Product.ProductMetadataClass
                {
                    Description = src.Description,
                    Color = src.Color,
                    Warranty = src.Warranty
                }));

            CreateMap<UpdateProductCommand, NextErp.Domain.Entities.Product>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Variations, opt => opt.Ignore())
                .ForMember(dest => dest.VariationOptions, opt => opt.Ignore())
                .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
                .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new NextErp.Domain.Entities.Product.ProductMetadataClass
                {
                    Description = src.Description,
                    Color = src.Color,
                    Warranty = src.Warranty
                }));

            // ===== Variation Mappings =====
            
            // VariationOption Entity -> Response DTO
            CreateMap<VariationOption, DTOs.ProductVariation.Response.VariationOptionDto>()
                .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values.OrderBy(v => v.DisplayOrder)));

            // VariationValue Entity -> Response DTO
            CreateMap<VariationValue, DTOs.ProductVariation.Response.VariationValueDto>();

            // ProductVariant Entity -> Response DTO
            CreateMap<ProductVariant, DTOs.ProductVariation.Response.ProductVariantDto>()
                .ForMember(dest => dest.VariationValues, opt => opt.MapFrom(src => src.VariationValues));
        }
    }
}
