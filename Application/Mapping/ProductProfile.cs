using Application.DTOs;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    class ProductProfile : Profile
    {

        public ProductProfile()
        {
            CreateMap<Product,ProductDto>().ReverseMap();
            CreateMap<ProductResponse,Product>()
                  .ForMember(dest => dest.Size, opt => opt.MapFrom(src => src.Size))

                .ReverseMap();
        }
    }
}
