using AutoMapper;
using TSWMS.ProductService.Api.Dto;
using TSWMS.ProductService.Shared.Models;

namespace TSWMS.ProductService.Api.MappingProfiles;

public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        // Maps Product to ProductDto and the other way around
        CreateMap<Product, ProductDto>();
        CreateMap<ProductDto, Product>();

        // Maps ProductPrice to ProductPriceDto and the other way around
        CreateMap<ProductPrice, ProductPriceDto>();
        CreateMap<ProductPriceDto, ProductPrice>();

        // Maps Product to ProductPriceDto
        CreateMap<Product, ProductPriceDto>()
            .ForMember(dest => dest.UnitPrice, opt => opt.MapFrom(src => src.Price));
    }
}
