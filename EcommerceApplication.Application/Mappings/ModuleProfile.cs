using AutoMapper;
using EcommerceApplicationWeb.Domain.Entities;
using EcommerceApplicationWeb.Application.DTOs;

namespace EcommerceApplicationWeb.Application.Mappings
{
    public class ModuleProfile : Profile
    {
        public ModuleProfile()
        {
            CreateMap<Module, ModuleResponseDto>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new ModuleMetadataDto
                {
                    Author = src.Metadata.Author,
                    Website = src.Metadata.Website,
                    Dependencies = src.Metadata.Dependencies,
                    ConfigurationUrl = src.Metadata.ConfigurationUrl
                }));

            CreateMap<ModuleRequestDto, Module>()
                .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new Module.ModuleMetadata
                {
                    Author = src.Metadata.Author,
                    Website = src.Metadata.Website,
                    Dependencies = src.Metadata.Dependencies,
                    ConfigurationUrl = src.Metadata.ConfigurationUrl
                }));
        }
    }
}
