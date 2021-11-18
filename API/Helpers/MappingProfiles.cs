﻿using API.DTOs;
using AutoMapper;
using Core.Entities;

namespace API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Product, ProductReturnDto>()
                .ForMember(p => p.ProductType, o => o.MapFrom(s => s.ProductType.Name))
                .ForMember(p => p.ProductBrand, o => o.MapFrom(s => s.ProductBrand.Name))
                .ForMember(p => p.PictureUrl, o => o.MapFrom<ProductUrlResolver>());
        }
    }
}
