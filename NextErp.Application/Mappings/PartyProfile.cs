using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class PartyProfile : Profile
    {
        public PartyProfile()
        {
            // Request DTO -> Command
            CreateMap<Party.Request.Create.Single, CreatePartyCommand>()
                .ConstructUsing(dto => new CreatePartyCommand(
                    dto.Title, dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Address,
                    dto.ContactPerson, dto.LoyaltyCode, dto.NationalId, dto.VatNumber, dto.TaxId,
                    dto.Notes, dto.PartyType, dto.IsActive));

            CreateMap<Party.Request.Update.Single, UpdatePartyCommand>()
                .ConstructUsing(dto => new UpdatePartyCommand(
                    dto.Id, dto.Title, dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.Address,
                    dto.ContactPerson, dto.LoyaltyCode, dto.NationalId, dto.VatNumber, dto.TaxId,
                    dto.Notes, dto.PartyType, dto.IsActive));

            // Entity -> Response DTOs
            CreateMap<Entities.Party, Party.Response.Get.Single>();
            CreateMap<Entities.Party, Party.Response.Create.Single>();
            CreateMap<Entities.Party, Party.Response.Update.Single>();
        }
    }
}
