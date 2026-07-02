using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Tenant;
using TenantEntity = NextErp.Domain.Entities.Tenant;

namespace NextErp.Application.Mapping;

// UseStaticMapper pulls in BranchMappers so the nested Branches collection on TenantResponse
// is mapped through BranchMappers.ToResponse (applying the Branch Title -> Name rename and
// metadata mapping). This is the coupling between the two modules.
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
[UseStaticMapper(typeof(BranchMappers))]
internal static partial class TenantMappers
{
    // ===== Request DTOs -> Entity =====

    // Create Request -> Entity (new). Name -> Title rename.
    [MapProperty(nameof(CreateTenantRequest.Name), nameof(TenantEntity.Title))]
    internal static partial TenantEntity ToEntity(this CreateTenantRequest request);

    // Update Request -> Entity (in-place). Name -> Title rename.
    [MapProperty(nameof(UpdateTenantRequest.Name), nameof(TenantEntity.Title))]
    internal static partial void ApplyTo(this UpdateTenantRequest request, TenantEntity entity);

    // ===== Entity -> Response DTOs =====

    // Title -> Name rename. Branches is mapped via BranchMappers.ToResponse (see UseStaticMapper).
    [MapProperty(nameof(TenantEntity.Title), nameof(TenantResponse.Name))]
    internal static partial TenantResponse ToResponse(this TenantEntity entity);

    [MapProperty(nameof(TenantEntity.Title), nameof(CreateTenantResponse.Name))]
    internal static partial CreateTenantResponse ToCreateResponse(this TenantEntity entity);

    [MapProperty(nameof(TenantEntity.Title), nameof(UpdateTenantResponse.Name))]
    internal static partial UpdateTenantResponse ToUpdateResponse(this TenantEntity entity);

    // ===== Metadata Mappings =====

    internal static partial TenantMetadataRequest ToMetadataRequest(this TenantEntity.TenantMetadata metadata);

    internal static partial TenantEntity.TenantMetadata ToMetadataEntity(this TenantMetadataRequest metadata);
}
