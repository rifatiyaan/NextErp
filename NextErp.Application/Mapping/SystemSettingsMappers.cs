using Riok.Mapperly.Abstractions;
using NextErp.Application.DTOs.SystemSettings;
using DomainSystemSettings = NextErp.Domain.Entities.SystemSettings;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class SystemSettingsMappers
{
    internal static partial SystemSettingsResponse ToResponse(this DomainSystemSettings e);
}

// Partial update: a null field in the request means "leave the existing entity
// value unchanged" so callers can PATCH a single property without clearing the
// rest. AllowNullPropertyAssignment = false makes Mapperly emit a null guard
// per member (if (r.X != null) e.X = r.X;). Id, TenantId, Title, CreatedAt and
// UpdatedAt have no counterpart on the request, so RequiredMappingStrategy.None
// leaves them untouched.
[Mapper(AllowNullPropertyAssignment = false, RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class SystemSettingsUpdateMapper
{
    internal static partial void ApplyTo(this UpdateSystemSettingsRequest r, DomainSystemSettings e);
}
