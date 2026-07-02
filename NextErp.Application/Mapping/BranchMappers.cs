using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.Branch;
using BranchEntity = NextErp.Domain.Entities.Branch;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class BranchMappers
{
    // ===== Request DTOs -> Entity =====

    // Create Request -> Entity (new). Name -> Title rename.
    [MapProperty(nameof(CreateBranchRequest.Name), nameof(BranchEntity.Title))]
    internal static partial BranchEntity ToEntity(this CreateBranchRequest request);

    // Update Request -> Entity (in-place). Name -> Title rename.
    [MapProperty(nameof(UpdateBranchRequest.Name), nameof(BranchEntity.Title))]
    internal static partial void ApplyTo(this UpdateBranchRequest request, BranchEntity entity);

    // ===== Entity -> Response DTOs =====

    // Title -> Name rename. Tenant nav has no source on the entity, so it is left unmapped.
    [MapProperty(nameof(BranchEntity.Title), nameof(BranchResponse.Name))]
    internal static partial BranchResponse ToResponse(this BranchEntity entity);

    [MapProperty(nameof(BranchEntity.Title), nameof(CreateBranchResponse.Name))]
    internal static partial CreateBranchResponse ToCreateResponse(this BranchEntity entity);

    [MapProperty(nameof(BranchEntity.Title), nameof(UpdateBranchResponse.Name))]
    internal static partial UpdateBranchResponse ToUpdateResponse(this BranchEntity entity);

    // ===== Metadata Mappings =====

    // Entity metadata has no Email; nothing to map to the DTO's Email.
    [MapperIgnoreTarget(nameof(BranchMetadataRequest.Email))]
    internal static partial BranchMetadataRequest ToMetadataRequest(this BranchEntity.BranchMetadata metadata);

    // DTO -> Entity: DTO.Email has no entity target; left unmapped under RequiredMappingStrategy.None.
    internal static partial BranchEntity.BranchMetadata ToMetadataEntity(this BranchMetadataRequest metadata);
}
