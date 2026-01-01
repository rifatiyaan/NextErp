using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class SupplierProfile : Profile
    {
        public SupplierProfile()
        {
            // Command -> Entity
            CreateMap<CreateSupplierCommand, Entities.Supplier>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseInvoices, opt => opt.Ignore());

            CreateMap<UpdateSupplierCommand, Entities.Supplier>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseInvoices, opt => opt.Ignore());

            // Entity -> Response DTOs
            CreateMap<Entities.Supplier, NextErp.Application.DTOs.Supplier.Response.Get.Single>();
            CreateMap<Entities.Supplier, NextErp.Application.DTOs.Supplier.Response.Create.Single>();
            CreateMap<Entities.Supplier, NextErp.Application.DTOs.Supplier.Response.Update.Single>();

            // Metadata mappings
            CreateMap<Entities.Supplier.SupplierMetadata, NextErp.Application.DTOs.Supplier.Request.Metadata>()
                .ReverseMap();

            // Request DTO -> Commands
            CreateMap<NextErp.Application.DTOs.Supplier.Request.Create.Single, CreateSupplierCommand>()
                .ConstructUsing(dto => new CreateSupplierCommand(
                    dto.Title,
                    dto.ContactPerson,
                    dto.Phone,
                    dto.Email,
                    dto.Address,
                    dto.IsActive
                ));

            CreateMap<NextErp.Application.DTOs.Supplier.Request.Update.Single, UpdateSupplierCommand>()
                .ConstructUsing(dto => new UpdateSupplierCommand(
                    dto.Id,
                    dto.Title,
                    dto.ContactPerson,
                    dto.Phone,
                    dto.Email,
                    dto.Address,
                    dto.IsActive
                ));
        }
    }
}
