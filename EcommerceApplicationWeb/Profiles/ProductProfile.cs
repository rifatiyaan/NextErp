using AutoMapper;
using EcommerceApplicationWeb.Application.DTOs;
using EcommerceApplicationWeb.Domain.Entities;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductResponseDto>()
                .ReverseMap();
            CreateMap<ProductRequestDto, Product>()
                .ReverseMap();
            CreateMap<Product.ProductMetadataClass, ProductMetadataDto>().ReverseMap();
        }
    }
}
