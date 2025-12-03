using AutoMapper;
using EcommerceApplicationWeb.Application.DTOs;
using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class MenuProfile : Profile
    {
        public MenuProfile()
        {
            CreateMap<MenuItem, MenuItemResponseDto>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new MenuItemMetadataDto
                {
                    Roles = src.Metadata.Roles,
                    BadgeText = src.Metadata.BadgeText,
                    BadgeColor = src.Metadata.BadgeColor,
                    OpenInNewTab = src.Metadata.OpenInNewTab,
                    Description = src.Metadata.Description
                }));

            CreateMap<MenuItemRequestDto, MenuItem>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new MenuItem.MenuItemMetadata
                {
                    Roles = src.Metadata.Roles,
                    BadgeText = src.Metadata.BadgeText,
                    BadgeColor = src.Metadata.BadgeColor,
                    OpenInNewTab = src.Metadata.OpenInNewTab,
                    Description = src.Metadata.Description
                }));
        }
    }
}
