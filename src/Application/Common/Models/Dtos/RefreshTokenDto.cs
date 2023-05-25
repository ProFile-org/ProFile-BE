using Application.Common.Mappings;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Models.Dtos;

public class RefreshTokenDto : IMapFrom<RefreshToken>
{
    public Guid Token { get; set; }
    public string JwtId { get; set; } = null!;
    public DateTime CreationDateTime { get; set; }
    public DateTime ExpiryDateTime { get; set; }
    public bool IsUsed { get; set; }
    public bool IsInvalidated { get; set; }
    public Guid UserId { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<RefreshToken, RefreshTokenDto>()
            .ForMember(dest => dest.CreationDateTime,
                opt => opt.MapFrom(src => src.CreationDateTime.ToDateTimeUnspecified()))
            .ForMember(dest => dest.ExpiryDateTime,
                opt => opt.MapFrom(src => src.ExpiryDateTime.ToDateTimeUnspecified()))
            .ForMember(dest => dest.UserId,
                opt => opt.MapFrom(src => src.User.Id));
    }
}