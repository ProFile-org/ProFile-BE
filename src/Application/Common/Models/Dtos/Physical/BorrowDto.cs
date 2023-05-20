using Application.Common.Mappings;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class BorrowDto : IMapFrom<Borrow>
{
    public UserDto Borrower { get; set; }
    public DocumentDto Document { get; set; }
    public DateTime BorrowTime { get; set; }
    public DateTime DueTime { get; set; }
    public string Reason { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Borrow, BorrowDto>()
            .ForMember(dest => dest.BorrowTime,
                opt => opt.MapFrom(src => src.BorrowTime.ToDateTimeUnspecified()))
            .ForMember(dest => dest.DueTime,
                opt => opt.MapFrom(src => src.DueTime.ToDateTimeUnspecified()));
    }
}