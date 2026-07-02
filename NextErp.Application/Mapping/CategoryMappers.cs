using Riok.Mapperly.Abstractions;
using NextErp.Application.Commands;
using NextErp.Application.DTOs.Category;
using CategoryEntity = NextErp.Domain.Entities.Category;
using CategoryAssetEntity = NextErp.Domain.Entities.Category.CategoryAsset;
using CategoryMetadataEntity = NextErp.Domain.Entities.Category.CategoryMetadataClass;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class CategoryMappers
{
    // ===== Entity -> Response =====

    // Full Get response: Products projected to ProductResponse (CategoryProfile mapped
    // dest.Products from src.Products under MaxDepth(1)). In the GetCategoryById path the
    // nested products have no Category loaded, so no Category<->Product cycle occurs.
    // Default = false: ToResponseShallow is the mapping Mapperly reuses for any nested
    // Category -> CategoryResponse (there is none today), keeping this full variant out of
    // auto-selection so the two same-signature methods never clash.
    [UserMapping(Default = false)]
    internal static CategoryResponse ToResponse(this CategoryEntity entity)
    {
        var response = ToResponseShallow(entity);
        response.Products = entity.Products != null
            ? entity.Products.Select(p => p.ToResponse()).ToList()
            : null;
        return response;
    }

    // Shallow response: every scalar + Metadata + Assets, but WITHOUT the nested Products
    // collection. Used by ProductMappers for the Product.Category nesting so the graph can
    // never recurse Category -> Products -> Category.
    [MapperIgnoreTarget(nameof(CategoryResponse.Products))]
    internal static partial CategoryResponse ToResponseShallow(this CategoryEntity entity);

    // CategoryMetadataClass <-> CategoryMetadataRequest (CategoryProfile had this with ReverseMap).
    internal static partial CategoryMetadataRequest ToMetadataResponse(this CategoryMetadataEntity metadata);

    // CategoryAsset entity -> CategoryAssetRequest (used by the response Assets list).
    internal static partial CategoryAssetRequest ToAssetResponse(this CategoryAssetEntity asset);

    // ===== Request DTO -> Command (ported from CategoryProfile ConstructUsing) =====

    internal static CreateCategoryCommand ToCommand(this CreateCategoryRequest dto) =>
        new(
            dto.Title,
            dto.Description,
            dto.ParentId,
            dto.Assets != null
                ? dto.Assets.Select(a => new CategoryAsset(
                    a.Filename,
                    a.Url,
                    a.Type,
                    a.Size,
                    a.UploadedAt)).ToList()
                : new List<CategoryAsset>());

    // CategoryProfile used the route id via context.Items["Id"]; the controller now sets
    // dto.Id before calling ToCommand, so we read dto.Id directly.
    internal static UpdateCategoryCommand ToCommand(this UpdateCategoryRequest dto) =>
        new(
            dto.Id,
            dto.Title,
            dto.Description,
            dto.ParentId,
            dto.IsActive,
            dto.Assets != null
                ? dto.Assets.Select(a => new CategoryAsset(
                    a.Filename,
                    a.Url,
                    a.Type,
                    a.Size,
                    a.UploadedAt)).ToList()
                : new List<CategoryAsset>());

    // ===== Command -> Entity (ported from CategoryProfile Command->Entity maps) =====
    // Assets projected element-wise. IsActive is deliberately NOT set on create so the DB
    // default applies (CategoryProfile ignored it); the create handler sets IsActive=true
    // explicitly. Id/audit/tenant/branch/navigation/Metadata are managed elsewhere.

    internal static CategoryEntity ToEntity(this CreateCategoryCommand command)
    {
        return new CategoryEntity
        {
            Title = command.Title,
            Description = command.Description,
            ParentId = command.ParentId,
            Assets = ToAssetEntities(command.Assets),
        };
    }

    internal static void ApplyTo(this UpdateCategoryCommand command, CategoryEntity entity)
    {
        entity.Title = command.Title;
        entity.Description = command.Description;
        entity.ParentId = command.ParentId;
        entity.IsActive = command.IsActive;
        entity.Assets = ToAssetEntities(command.Assets);
    }

    private static List<CategoryAssetEntity> ToAssetEntities(List<CategoryAsset>? assets) =>
        assets != null
            ? assets.Select(a => new CategoryAssetEntity
            {
                Filename = a.Filename,
                Url = a.Url,
                Type = a.Type,
                Size = a.Size,
                UploadedAt = a.UploadedAt,
            }).ToList()
            : new List<CategoryAssetEntity>();
}
