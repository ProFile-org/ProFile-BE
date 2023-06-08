using Application.Common.Mappings;
using Domain.Entities.Digital;

namespace Application.Common.Models.Dtos.Digital;

public class EntryDto : IMapFrom<Entry>
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public FileDto? File { get; set; }
}