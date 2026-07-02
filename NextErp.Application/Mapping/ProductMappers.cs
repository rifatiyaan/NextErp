using Riok.Mapperly.Abstractions;
using NextErp.Application.Commands;
using NextErp.Application.DTOs.Product;
using NextErp.Application.DTOs.ProductVariation;
using ProductEntity = NextErp.Domain.Entities.Product;
using ProductMetadataEntity = NextErp.Domain.Entities.Product.ProductMetadataClass;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class ProductMappers
{
    // ===== Entity -> Response =====
    // Hand-written to reproduce ProductProfile exactly: computed HasVariations,
    // ordered VariationOptions/Images, empty-collection->null collapsing, unit
    // flattening, and the Category nesting (mapped shallow to avoid Category<->Product
    // recursion, matching the profile's MaxDepth(3) / the query's Include shape).
    internal static ProductResponse ToResponse(this ProductEntity entity)
    {
        return new ProductResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Code = entity.Code,
            Price = entity.Price,
            CategoryId = entity.CategoryId,
            ImageUrl = entity.ImageUrl,
            ParentId = entity.ParentId,
            UnitOfMeasureId = entity.UnitOfMeasureId,
            UnitAbbreviation = entity.UnitOfMeasure != null ? entity.UnitOfMeasure.Abbreviation : null,
            UnitTitle = entity.UnitOfMeasure != null ? (entity.UnitOfMeasure.Title ?? entity.UnitOfMeasure.Name) : null,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            TenantId = entity.TenantId,
            BranchId = entity.BranchId,
            Metadata = ToMetadataResponse(entity.Metadata),
            Category = entity.Category != null ? CategoryMappers.ToResponseShallow(entity.Category) : null,
            HasVariations = entity.HasVariations ||
                (entity.ProductVariationOptions != null && entity.ProductVariationOptions.Any()),
            VariationOptions = (entity.ProductVariationOptions != null && entity.ProductVariationOptions.Any())
                ? entity.ProductVariationOptions
                    .OrderBy(pvo => pvo.DisplayOrder)
                    .Select(pvo => pvo.VariationOption.ToResponse())
                    .ToList()
                : null,
            ProductVariants = (entity.ProductVariants != null && entity.ProductVariants.Any())
                ? entity.ProductVariants.Select(pv => pv.ToResponse()).ToList()
                : null,
            Images = (entity.ProductImages != null && entity.ProductImages.Count > 0)
                ? entity.ProductImages
                    .OrderBy(pi => pi.DisplayOrder)
                    .Select(pi => new ProductImageItemResponse
                    {
                        Id = pi.Id,
                        Url = pi.Url,
                        DisplayOrder = pi.DisplayOrder,
                        IsThumbnail = pi.IsThumbnail,
                    })
                    .ToList()
                : null,
        };
    }

    // ProductMetadataClass -> ProductMetadataRequest. CategoryId has no entity source
    // (ProductProfile ignored it); Description/Color/Warranty copy by name.
    internal static partial ProductMetadataRequest ToMetadataResponse(this ProductMetadataEntity metadata);

    // ===== Request DTO -> Command (ported from ProductProfile.MapCreateProductCommand /
    // MapUpdateProductCommand — hand-written, NOT Mapperly-generated). =====

    internal static CreateProductCommand ToCommand(this CreateProductRequest dto)
    {
        IReadOnlyList<GalleryResolvedSlot> gallery =
            dto.ResolvedGallery != null && dto.ResolvedGallery.Count > 0
                ? dto.ResolvedGallery
                : string.IsNullOrWhiteSpace(dto.ImageUrl)
                    ? Array.Empty<GalleryResolvedSlot>()
                    : new List<GalleryResolvedSlot> { new(dto.ImageUrl.Trim(), true) };

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

    internal static UpdateProductCommand ToCommand(this UpdateProductRequest dto) =>
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

    // ===== Command -> Entity (ported from ProductProfile Command->Entity maps) =====
    // Hand-written so the Metadata sub-object (Description/Color/Warranty) is built and
    // the "blank Code keeps existing" condition on update is honoured. Id/audit/branch/
    // navigation/HasVariations/IsActive are managed by the handlers (left unset here).

    internal static ProductEntity ToEntity(this CreateProductCommand command)
    {
        var entity = new ProductEntity
        {
            Title = command.Title,
            Code = command.Code!,
            ParentId = command.ParentId,
            CategoryId = command.CategoryId,
            Price = command.Price,
            ImageUrl = command.ImageUrl,
            UnitOfMeasureId = command.UnitOfMeasureId,
            Metadata = new ProductMetadataEntity
            {
                Description = command.Description,
                Color = command.Color,
                Warranty = command.Warranty,
            },
        };
        return entity;
    }

    internal static void ApplyTo(this UpdateProductCommand command, ProductEntity entity)
    {
        entity.Title = command.Title;
        // ProductProfile: Code mapped only when non-blank (Condition).
        if (!string.IsNullOrWhiteSpace(command.Code))
            entity.Code = command.Code!;
        entity.ParentId = command.ParentId;
        entity.CategoryId = command.CategoryId;
        entity.Price = command.Price;
        entity.IsActive = command.IsActive;
        entity.ImageUrl = command.ImageUrl;
        entity.UnitOfMeasureId = command.UnitOfMeasureId;
        entity.Metadata = new ProductMetadataEntity
        {
            Description = command.Description,
            Color = command.Color,
            Warranty = command.Warranty,
        };
    }

    internal static ProductEntity ToEntity(this CreateProductWithVariationsCommand command)
    {
        var entity = new ProductEntity
        {
            Title = command.Title,
            Code = command.Code!,
            ParentId = command.ParentId,
            CategoryId = command.CategoryId,
            Price = command.Price,
            ImageUrl = command.ImageUrl,
            UnitOfMeasureId = command.UnitOfMeasureId,
            // ProductProfile mapped HasVariations => true for the WithVariations command.
            HasVariations = true,
            Metadata = new ProductMetadataEntity
            {
                Description = command.Description,
                Color = command.Color,
                Warranty = command.Warranty,
            },
        };
        return entity;
    }

    internal static void ApplyTo(this UpdateProductWithVariationsCommand command, ProductEntity entity)
    {
        entity.Title = command.Title;
        if (!string.IsNullOrWhiteSpace(command.Code))
            entity.Code = command.Code!;
        entity.ParentId = command.ParentId;
        entity.CategoryId = command.CategoryId;
        entity.Price = command.Price;
        entity.IsActive = command.IsActive;
        entity.ImageUrl = command.ImageUrl;
        entity.UnitOfMeasureId = command.UnitOfMeasureId;
        entity.HasVariations = true;
        entity.Metadata = new ProductMetadataEntity
        {
            Description = command.Description,
            Color = command.Color,
            Warranty = command.Warranty,
        };
    }
}
