using Riok.Mapperly.Abstractions;
using NextErp.Application.Commands;
using NextErp.Application.DTOs.Party;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mapping;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
internal static partial class PartyMappers
{
    // Request DTO -> Command (positional records; mapped by property/parameter name)
    internal static partial CreatePartyCommand ToCommand(this CreatePartyRequest r);

    internal static partial UpdatePartyCommand ToCommand(this UpdatePartyRequest r);

    // Entity -> Response DTOs
    internal static partial PartyResponse ToResponse(this Entities.Party e);

    internal static partial CreatePartyResponse ToCreateResponse(this Entities.Party e);

    internal static partial UpdatePartyResponse ToUpdateResponse(this Entities.Party e);
}
