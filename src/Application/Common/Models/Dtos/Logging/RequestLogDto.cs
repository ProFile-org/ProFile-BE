using Application.Common.Mappings;
using Application.Common.Models.Dtos.Physical;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Logging;

namespace Application.Common.Models.Dtos.Logging;

public class RequestLogDto : IMapFrom<RequestLog>
{
    public Guid Id { get; set; }
    public string Action { get; set; } = null!;
    public DocumentDto? Object { get; set; }
    public DateTime Time { get; set; }
    public UserDto User { get; set; } = null!;
    public string Reason { get; set; } = null!;
    public string Type { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RequestLog, RequestLogDto>()
            .ForMember(dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.ToDateTimeUnspecified()))
            .ForMember(dest => dest.Object,
                opt => opt.MapFrom(src => src.Object))
            .ForMember(dest => dest.Type,
                opt => opt.MapFrom(src => src.Type.ToString()));
    }
}