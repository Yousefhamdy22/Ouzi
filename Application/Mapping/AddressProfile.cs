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
    class AddressProfile : Profile
    {
        public AddressProfile() {
            // In your mapping profile (AutoMapper)
            CreateMap<AddressDto, Address>()
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Don't map UserId from DTO
                .ForMember(dest => dest.UserId, opt => opt.MapFrom((src, dest, destMember, context) =>
                    context.Items["UserId"])); // Set from context // intial to building user services 

            CreateMap<Address , AddressDto>().ReverseMap();

            

        }


    }
}
