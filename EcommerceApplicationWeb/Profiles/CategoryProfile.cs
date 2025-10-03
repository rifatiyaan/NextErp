using AutoMapper;
using EcommerceApplicationWeb.Application.DTOs;
using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryResponseDto>()
                .ReverseMap();
            CreateMap<CategoryRequestDto, Category>()
                .ReverseMap();
            CreateMap<Category.CategoryMetadataClass, CategoryMetadataDto>().ReverseMap();
        }
    }
}
