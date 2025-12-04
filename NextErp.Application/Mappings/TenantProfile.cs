using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class TenantProfile : Profile
    {
        public TenantProfile()
        {
            // Entity <-> Response DTO
            CreateMap<Tenant, TenantResponseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                .ReverseMap()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name));

            // Request DTO -> Entity
            CreateMap<TenantRequestDto, Tenant>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Branches, opt => opt.Ignore());

            // Metadata mappings
            CreateMap<Tenant.TenantMetadata, TenantMetadataDto>()
                .ReverseMap();
        }
    }
}
