using AutoMapper;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class StockProfile : Profile
    {
        public StockProfile()
        {
            CreateMap<Entities.Stock, NextErp.Application.DTOs.Stock.Response.Single>()
                .ForMember(dest => dest.ProductVariantId, opt => opt.MapFrom(src => src.ProductVariantId))
                .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src =>
                    src.ProductVariant != null ? src.ProductVariant.ProductId : 0))
                .ForMember(dest => dest.ProductTitle, opt => opt.MapFrom(src =>
                    src.ProductVariant != null && src.ProductVariant.Product != null
                        ? src.ProductVariant.Product.Title
                        : "Unknown"))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src =>
                    src.ProductVariant != null && src.ProductVariant.Product != null
                        ? src.ProductVariant.Product.Code
                        : "N/A"))
                .ForMember(dest => dest.VariantSku, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Sku : ""))
                .ForMember(dest => dest.VariantTitle, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Title : ""));
        }
    }
}
