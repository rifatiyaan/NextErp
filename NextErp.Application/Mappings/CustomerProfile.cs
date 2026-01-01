using AutoMapper;
using NextErp.Application.Commands;
using NextErp.Application.DTOs;
using Entities = NextErp.Domain.Entities;

namespace NextErp.Application.Mappings
{
    public class CustomerProfile : Profile
    {
        public CustomerProfile()
        {
            // Command -> Entity
            CreateMap<CreateCustomerCommand, Entities.Customer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.SalesInvoices, opt => opt.Ignore());

            CreateMap<UpdateCustomerCommand, Entities.Customer>()
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.TenantId, opt => opt.Ignore())
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.Metadata, opt => opt.Ignore())
                .ForMember(dest => dest.SalesInvoices, opt => opt.Ignore());

            // Entity -> Response DTOs
            CreateMap<Entities.Customer, NextErp.Application.DTOs.Customer.Response.Get.Single>();
            CreateMap<Entities.Customer, NextErp.Application.DTOs.Customer.Response.Create.Single>();
            CreateMap<Entities.Customer, NextErp.Application.DTOs.Customer.Response.Update.Single>();

            // Metadata mappings
            CreateMap<Entities.Customer.CustomerMetadata, NextErp.Application.DTOs.Customer.Request.Metadata>()
                .ReverseMap();

            // Request DTO -> Commands
            CreateMap<NextErp.Application.DTOs.Customer.Request.Create.Single, CreateCustomerCommand>()
                .ConstructUsing(dto => new CreateCustomerCommand(
                    dto.Title,
                    dto.Email,
                    dto.Phone,
                    dto.Address,
                    dto.IsActive
                ));

            CreateMap<NextErp.Application.DTOs.Customer.Request.Update.Single, UpdateCustomerCommand>()
                .ConstructUsing(dto => new UpdateCustomerCommand(
                    dto.Id,
                    dto.Title,
                    dto.Email,
                    dto.Phone,
                    dto.Address,
                    dto.IsActive
                ));
        }
    }
}
