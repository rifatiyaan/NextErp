using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Module;
using DomainModule = NextErp.Domain.Entities.Module;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class ModuleMappers
{
    // ===== Request -> Entity =====
    // RequiredMappingStrategy.None + requests lacking Id/CreatedAt/UpdatedAt/
    // InstalledAt/TenantId/BranchId/Parent/Children/ParentId means those entity
    // members are simply left at their defaults — no [MapperIgnoreTarget] needed.

    internal static partial DomainModule ToEntity(this CreateModuleRequest request);

    // Children/ParentId are NOT mapped here: the bulk handler flattens the
    // hierarchy itself (adds each child as its own row with an explicit ParentId).
    // Letting Mapperly populate entity.Children would make EF insert duplicate
    // child rows — matches the old profile's Ignore(Children)/Ignore(ParentId).
    [MapperIgnoreTarget(nameof(DomainModule.Children))]
    [MapperIgnoreTarget(nameof(DomainModule.ParentId))]
    internal static partial DomainModule ToEntity(this CreateModuleHierarchicalRequest request);

    // In-place update: handler loads the existing tracked entity, then applies the request.
    // Id and InstalledAt are present on the request but were Ignored by the old
    // profile, so keep them off the in-place copy (don't reassign the key; don't
    // let an update clobber InstalledAt). CreatedAt/UpdatedAt/TenantId/BranchId/
    // Parent/Children have no matching source member, so None leaves them as-is.
    [MapperIgnoreTarget(nameof(DomainModule.Id))]
    [MapperIgnoreTarget(nameof(DomainModule.InstalledAt))]
    internal static partial void ApplyTo(this UpdateModuleRequest request, DomainModule entity);

    // ===== Entity -> Response =====
    // Distinct method names because C# extension calls cannot be resolved by
    // return type alone (all four sources are DomainModule).

    internal static partial ModuleResponse ToResponse(this DomainModule entity);

    internal static partial CreateModuleResponse ToCreateResponse(this DomainModule entity);

    internal static partial CreateModuleHierarchicalResponse ToCreateHierarchicalResponse(this DomainModule entity);

    internal static partial UpdateModuleResponse ToUpdateResponse(this DomainModule entity);

    // ===== Metadata (both directions) =====
    // Mapperly uses these for the nested Metadata property on the maps above.

    internal static partial DomainModule.ModuleMetadata ToEntity(this ModuleMetadataRequest request);

    internal static partial ModuleMetadataRequest ToRequest(this DomainModule.ModuleMetadata metadata);
}
