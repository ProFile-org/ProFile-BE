using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Models.Dtos;

public class DepartmentDto : BaseDto, IMapFrom<Department>
{
    public string Name { get; set; } = null!;
}