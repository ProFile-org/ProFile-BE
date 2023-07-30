using Application.Common.Mappings;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;

namespace Application.Common.Models.Dtos.Physical;

public class BorrowDto : BaseDto, IMapFrom<Borrow>
{
    public Guid BorrowerId { get; set; }
    public Guid DocumentId { get; set; }
    public DateTime BorrowTime { get; set; }
    public DateTime DueTime { get; set; }
    public DateTime ActualReturnTime { get; set; }
    public string BorrowReason { get; set; } = null!;
    public string StaffReason { get; set; } = null!;
    public string Status { get; set; } = null!;

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Borrow, BorrowDto>()
            .ForMember(dest => dest.BorrowerId,
                opt => opt.MapFrom(src => src.Borrower.Id))
            .ForMember(dest => dest.DocumentId,
                               opt => opt.MapFrom(src => src.Document.Id))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.BorrowTime,
                opt => opt.MapFrom(src => src.BorrowTime.ToDateTimeUnspecified()))
            .ForMember(dest => dest.DueTime,
                opt => opt.MapFrom(src => src.DueTime.ToDateTimeUnspecified()))
            .ForMember(dest => dest.ActualReturnTime,
                opt => opt.MapFrom(src => src.ActualReturnTime.ToDateTimeUnspecified()));
    }
}