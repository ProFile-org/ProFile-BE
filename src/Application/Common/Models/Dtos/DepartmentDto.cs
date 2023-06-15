﻿using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Models.Dtos;

public class DepartmentDto : BaseDto, IMapFrom<Department>
{
    public string Name { get; set; } = null!;
    public Guid? RoomId { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<Department, DepartmentDto>()
            .ForMember(dest => dest.RoomId,
                opt => opt.MapFrom(src => src.Room!.Id));
    }
}