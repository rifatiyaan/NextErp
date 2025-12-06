using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
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
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.InstalledAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore());

            // Metadata mappings
            CreateMap<Module.ModuleMetadata, ModuleMetadataDto>()
                .ReverseMap();
        }
    }
}
