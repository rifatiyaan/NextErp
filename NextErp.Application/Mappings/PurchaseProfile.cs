using AutoMapper;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class PurchaseProfile : Profile
    {
        public PurchaseProfile()
        {
            // Entity -> Response DTOs
            CreateMap<Entities.Purchase, NextErp.Application.DTOs.Purchase.Response.Get.Single>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Title : "Unknown"))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items));

            CreateMap<Entities.PurchaseItem, NextErp.Application.DTOs.Purchase.Response.Get.PurchaseItemResponse>()
                .ForMember(dest => dest.ProductTitle, opt => opt.MapFrom(src =>
                    src.ProductVariant != null && src.ProductVariant.Product != null
                        ? src.ProductVariant.Product.Title
                        : "Unknown"))
                .ForMember(dest => dest.VariantSku, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Sku : ""))
                .ForMember(dest => dest.VariantTitle, opt => opt.MapFrom(src => src.ProductVariant != null ? src.ProductVariant.Title : ""))
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => src.Metadata));

            CreateMap<Entities.Purchase, NextErp.Application.DTOs.Purchase.Response.Create.Single>();

            // Metadata mappings
            CreateMap<Entities.Purchase.PurchaseMetadata, NextErp.Application.DTOs.Purchase.Request.Metadata>()
                .ReverseMap();

            CreateMap<Entities.PurchaseItem.PurchaseItemMetadata, NextErp.Application.DTOs.Purchase.Response.Get.PurchaseItemMetadata>()
                .ReverseMap();

            CreateMap<Entities.PurchaseItem.PurchaseItemMetadata, NextErp.Application.DTOs.Purchase.Request.Create.PurchaseItemMetadata>()
                .ReverseMap();
        }
    }
}
