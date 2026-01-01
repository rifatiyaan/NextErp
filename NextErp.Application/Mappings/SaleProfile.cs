using AutoMapper;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class SaleProfile : Profile
    {
        public SaleProfile()
        {
            // Entity -> Response DTOs
            CreateMap<Entities.Sale, NextErp.Application.DTOs.Sale.Response.Get.Single>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Title : "Unknown"))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<Entities.SaleItem, NextErp.Application.DTOs.Sale.Response.Get.SaleItemResponse>()
                .ForMember(dest => dest.ProductTitle, opt => opt.MapFrom(src => src.Product != null ? src.Product.Title : "Unknown"));

            CreateMap<Entities.Sale, NextErp.Application.DTOs.Sale.Response.Create.Single>();

            // Metadata mappings
            CreateMap<Entities.Sale.SaleMetadata, NextErp.Application.DTOs.Sale.Request.Metadata>()
                .ReverseMap();
        }
    }
}
