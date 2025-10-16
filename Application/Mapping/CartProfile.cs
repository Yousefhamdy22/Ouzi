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
    public class CartProfile : Profile
    {
        public CartProfile()
        {
             
            
            CreateMap<Cart , CartDto>();

            CreateMap<CartItem, CartItemDto>();
                
            CreateMap<CartItemDto, CartItem>();

            CreateMap<CartItemCreateDto, CartItem>().ReverseMap();
        }

    }
}
