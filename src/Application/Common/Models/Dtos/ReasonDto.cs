using Application.Common.Mappings;
using Domain.Entities.Logging;
using Domain.Enums;

namespace Application.Common.Models.Dtos;

public class ReasonDto : IMapFrom<RequestLog>
{
    public RequestType Type { get; set; }
    public string Reason { get; set; } = null!;
}