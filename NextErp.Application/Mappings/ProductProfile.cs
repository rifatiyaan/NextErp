using AutoMapper;
using DTOs = NextErp.Application.DTOs;
using NextErp.Application.Commands;
using NextErp.Domain.Entities;
using ProductEntity = NextErp.Domain.Entities.Product;

namespace NextErp.Application.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        // ===== Request DTOs to Entity =====

        // Create Request -> Entity
        CreateMap<DTOs.Product.Request.Create.Single, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Variations, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
            .ForMember(dest => dest.UnitOfMeasure, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId ?? 0));

        // Update Request -> Entity
        CreateMap<DTOs.Product.Request.Update.Single, ProductEntity>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Variations, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
            .ForMember(dest => dest.UnitOfMeasure, opt => opt.Ignore())
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId ?? 0));

        // ===== Entity to Response DTOs =====

        // Entity -> Get Single Response
        CreateMap<ProductEntity, DTOs.Product.Response.Get.Single>()
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId))
            .ForMember(dest => dest.UnitOfMeasureId, opt => opt.MapFrom(src => src.UnitOfMeasureId))
            .ForMember(dest => dest.UnitAbbreviation, opt => opt.MapFrom(src => src.UnitOfMeasure != null ? src.UnitOfMeasure.Abbreviation : null))
            .ForMember(dest => dest.UnitTitle, opt => opt.MapFrom(src => src.UnitOfMeasure != null ? (src.UnitOfMeasure.Title ?? src.UnitOfMeasure.Name) : null))
            .ForMember(dest => dest.HasVariations, opt => opt.MapFrom(src =>
                src.HasVariations || (src.ProductVariationOptions != null && src.ProductVariationOptions.Any())))
            .ForMember(dest => dest.VariationOptions, opt => opt.MapFrom(src =>
                (src.ProductVariationOptions != null && src.ProductVariationOptions.Any())
                    ? src.ProductVariationOptions.OrderBy(pvo => pvo.DisplayOrder).Select(pvo => pvo.VariationOption).ToList()
                    : null))
            .ForMember(dest => dest.ProductVariants, opt => opt.MapFrom(src =>
                (src.ProductVariants != null && src.ProductVariants.Any())
                    ? src.ProductVariants.ToList()
                    : null))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src =>
                src.ProductImages != null && src.ProductImages.Count > 0
                    ? src.ProductImages
                        .OrderBy(pi => pi.DisplayOrder)
                        .Select(pi => new DTOs.Product.Response.Get.ProductImageItem
                        {
                            Id = pi.Id,
                            Url = pi.Url,
                            DisplayOrder = pi.DisplayOrder,
                            IsThumbnail = pi.IsThumbnail,
                        })
                        .ToList()
                    : null))
            .MaxDepth(3);

        // Entity -> Create Single Response
        CreateMap<ProductEntity, DTOs.Product.Response.Create.Single>()
            .MaxDepth(1);

        // Entity -> Update Single Response
        CreateMap<ProductEntity, DTOs.Product.Response.Update.Single>()
            .MaxDepth(1);

        // ===== Metadata Mappings =====

        CreateMap<ProductEntity.ProductMetadataClass, DTOs.Product.Request.Metadata>()
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore())
            .ReverseMap();

        // ===== Legacy Mappings (for backward compatibility during transition) =====

        // Request DTO -> Commands
        CreateMap<DTOs.Product.Request.Create.Single, CreateProductCommand>()
            .ConstructUsing(src => MapCreateProductCommand(src));

        CreateMap<DTOs.Product.Request.Update.Single, UpdateProductCommand>()
            .ConstructUsing(src => MapUpdateProductCommand(src));

        // Command -> Entity (for handlers)
        CreateMap<CreateProductCommand, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Variations, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.UnitOfMeasure, opt => opt.Ignore())
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new ProductEntity.ProductMetadataClass
            {
                Description = src.Description,
                Color = src.Color,
                Warranty = src.Warranty
            }));

        CreateMap<UpdateProductCommand, ProductEntity>()
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Variations, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.HasVariations, opt => opt.Ignore())
            .ForMember(dest => dest.UnitOfMeasure, opt => opt.Ignore())
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new ProductEntity.ProductMetadataClass
            {
                Description = src.Description,
                Color = src.Color,
                Warranty = src.Warranty
            }));

        // CreateProductWithVariationsCommand -> Product Entity
        CreateMap<CreateProductWithVariationsCommand, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Variations, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.HasVariations, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.UnitOfMeasure, opt => opt.Ignore())
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new ProductEntity.ProductMetadataClass
            {
                Description = src.Description,
                Color = src.Color,
                Warranty = src.Warranty
            }));

        // UpdateProductWithVariationsCommand -> Product Entity
        CreateMap<UpdateProductWithVariationsCommand, ProductEntity>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()) // Use existing entity's ID
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore())
            .ForMember(dest => dest.Parent, opt => opt.Ignore())
            .ForMember(dest => dest.Children, opt => opt.Ignore())
            .ForMember(dest => dest.Category, opt => opt.Ignore())
            .ForMember(dest => dest.Variations, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariants, opt => opt.Ignore())
            .ForMember(dest => dest.ProductImages, opt => opt.Ignore())
            .ForMember(dest => dest.HasVariations, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.UnitOfMeasure, opt => opt.Ignore())
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new ProductEntity.ProductMetadataClass
            {
                Description = src.Description,
                Color = src.Color,
                Warranty = src.Warranty
            }));

        // ===== Variation Mappings =====

        // Request DTO -> Entity (for creating/updating)
        CreateMap<DTOs.ProductVariation.Request.VariationOptionDto, VariationOption>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Values, opt => opt.Ignore())
            .ForMember(dest => dest.ProductVariationOptions, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore());

        CreateMap<DTOs.ProductVariation.Request.VariationValueDto, VariationValue>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.VariationOptionId, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore());

        CreateMap<DTOs.ProductVariation.Request.ProductVariantDto, ProductVariant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ProductId, opt => opt.Ignore())
            .ForMember(dest => dest.VariationValues, opt => opt.Ignore())
            .ForMember(dest => dest.Title, opt => opt.Ignore()) // Set manually from variation values
            .ForMember(dest => dest.Name, opt => opt.Ignore()) // Set manually from variation values
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.BranchId, opt => opt.Ignore());

        // Entity -> Response DTO
        CreateMap<VariationOption, DTOs.ProductVariation.Response.VariationOptionDto>()
            .ForMember(dest => dest.Values, opt => opt.MapFrom(src => src.Values.OrderBy(v => v.DisplayOrder)));

        CreateMap<VariationValue, DTOs.ProductVariation.Response.VariationValueDto>();

        CreateMap<ProductVariant, DTOs.ProductVariation.Response.ProductVariantDto>()
            .ForMember(dest => dest.AvailableQuantity, opt => opt.Ignore()) // populated by ProductVariantStockLookup
            .ForMember(dest => dest.VariationValues, opt => opt.MapFrom(src => src.VariationValues));
    }

    private static CreateProductCommand MapCreateProductCommand(DTOs.Product.Request.Create.Single dto)
    {
        IReadOnlyList<DTOs.Product.Request.GalleryResolvedSlot> gallery =
            dto.ResolvedGallery != null && dto.ResolvedGallery.Count > 0
                ? dto.ResolvedGallery
                : string.IsNullOrWhiteSpace(dto.ImageUrl)
                    ? Array.Empty<DTOs.Product.Request.GalleryResolvedSlot>()
                    : new List<DTOs.Product.Request.GalleryResolvedSlot>
                        { new(dto.ImageUrl.Trim(), true) };

        return new CreateProductCommand(
            dto.Title,
            dto.Code,
            dto.ParentId,
            dto.Metadata != null && dto.Metadata.CategoryId.HasValue ? dto.Metadata.CategoryId.Value : (dto.CategoryId ?? 0),
            dto.Price,
            dto.InitialStock,
            dto.IsActive,
            dto.ImageUrl,
            gallery,
            dto.Metadata != null ? dto.Metadata.Description : null,
            dto.Metadata != null ? dto.Metadata.Color : null,
            dto.Metadata != null ? dto.Metadata.Warranty : null,
            UnitOfMeasureId: dto.UnitOfMeasureId);
    }

    private static UpdateProductCommand MapUpdateProductCommand(DTOs.Product.Request.Update.Single dto) =>
        new(
            dto.Id,
            dto.Title,
            dto.Code,
            dto.ParentId,
            dto.Metadata != null && dto.Metadata.CategoryId.HasValue ? dto.Metadata.CategoryId.Value : (dto.CategoryId ?? 0),
            dto.Price,
            dto.IsActive,
            dto.ImageUrl,
            dto.ResolvedGallery,
            dto.ProductImageThumbnailUpdates != null && dto.ProductImageThumbnailUpdates.Count > 0
                ? dto.ProductImageThumbnailUpdates
                : null,
            dto.Metadata != null ? dto.Metadata.Description : null,
            dto.Metadata != null ? dto.Metadata.Color : null,
            dto.Metadata != null ? dto.Metadata.Warranty : null,
            UnitOfMeasureId: dto.UnitOfMeasureId);
}
