using AutoMapper;
using EcommerceApplicationWeb.Domain.Entities;
using EcommerceApplicationWeb.Application.DTOs;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class ModuleProfile : Profile
    {
        public ModuleProfile()
        {
            // Entity <-> Response DTO
            CreateMap<Module, ModuleResponseDto>()
                .ReverseMap();

            // Request DTO -> Entity
            CreateMap<ModuleRequestDto, Module>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.MenuItems, opt => opt.Ignore())
                .ForMember(dest => dest.InstalledAt, opt => opt.Ignore())
                .ForMember(dest => dest.IconUrl, opt => opt.Ignore());

            // Metadata mappings
            CreateMap<Module.ModuleMetadata, ModuleMetadataDto>()
                .ReverseMap();
        }
    }
}
