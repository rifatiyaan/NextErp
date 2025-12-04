using AutoMapper;
using EcommerceApplicationWeb.Application.DTOs;
using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class BranchProfile : Profile
    {
        public BranchProfile()
        {
            // Entity <-> Response DTO
            CreateMap<Branch, BranchResponseDto>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Title))
                .ReverseMap()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name));

            // Request DTO -> Entity
            CreateMap<BranchRequestDto, Branch>()
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Tenant, opt => opt.Ignore());

            // Metadata mappings
            CreateMap<Branch.BranchMetadata, BranchMetadataDto>()
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ReverseMap();
        }
    }
}
