using AutoMapper;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class StockProfile : Profile
    {
        public StockProfile()
        {
            // Entity -> Response DTOs (Stock is read-only from API perspective)
            CreateMap<Entities.Stock, NextErp.Application.DTOs.Stock.Response.Single>()
                .ForMember(dest => dest.ProductTitle, opt => opt.MapFrom(src => src.Product != null ? src.Product.Title : "Unknown"))
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : "N/A"));
        }
    }
}
