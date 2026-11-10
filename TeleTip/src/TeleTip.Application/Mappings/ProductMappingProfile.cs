using AutoMapper;
using MyProject.Application.Features.Products.Dtos;
using MyProject.Domain.Entities;

namespace MyProject.Application.Mappings
{
    public sealed class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductDto>().ReverseMap();
        }
    }
}
