using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Application.Rooms.Queries.GetEmptyContainersPaginated;

public record GetEmptyContainersPaginatedQuery : IRequest<PaginatedList<EmptyLockerDto>>
{
    public Guid RoomId { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
}

public class GetEmptyContainersPaginatedQueryHandler : IRequestHandler<GetEmptyContainersPaginatedQuery, PaginatedList<EmptyLockerDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetEmptyContainersPaginatedQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<EmptyLockerDto>> Handle(GetEmptyContainersPaginatedQuery request, CancellationToken cancellationToken)
    {
        var room = await _context.Rooms.FirstOrDefaultAsync(x => x.Id == request.RoomId, cancellationToken);
        if (room is null)
        {
            throw new KeyNotFoundException("Room does not exist");
        }
        
        var lockers = _context.Lockers
            .Where(x => x.Room.Id == request.RoomId
                        && x.IsAvailable
                        && x.Folders.Any(y => y.Capacity > y.NumberOfDocuments && y.IsAvailable))
            .ProjectTo<EmptyLockerDto>(_mapper.ConfigurationProvider)
            .AsEnumerable()
            .ToList();
        
        lockers.ForEach(x => x.Folders = x.Folders.Where(y => y.Slot > 0));

        var result = new PaginatedList<EmptyLockerDto>(lockers.ToList(), lockers.Count(), request.Page, request.Size);
        return result;
    }
} 