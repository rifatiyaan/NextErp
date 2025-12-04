using AutoMapper;
using NextErp.Application.DTOs;
using NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class MenuProfile : Profile
    {
        public MenuProfile()
        {
            // Entity <-> Response DTO
            CreateMap<MenuItem, MenuItemResponseDto>()
                .ReverseMap();

            // Request DTO -> Entity
            CreateMap<MenuItemRequestDto, MenuItem>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Parent, opt => opt.Ignore())
                .ForMember(dest => dest.Children, opt => opt.Ignore())
                .ForMember(dest => dest.Module, opt => opt.Ignore());

            // Metadata mappings
            CreateMap<MenuItem.MenuItemMetadata, MenuItemMetadataDto>()
                .ReverseMap();
        }
    }
}
