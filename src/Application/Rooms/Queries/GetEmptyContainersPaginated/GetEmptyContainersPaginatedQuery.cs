using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using Application.Common.Models.Dtos.Physical;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Entities.Physical;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        var result = await _context.Lockers
            .Include(x => x.Folders.Where(y => y.NumberOfDocuments < y.Capacity && y.IsAvailable))
            .Where(x => x.Room.Id == request.RoomId
                        && x.IsAvailable)
            .ProjectTo<EmptyLockerDto>(_mapper.ConfigurationProvider)
            .Where(x => x.NumberOfFreeFolders > 0)
            .OrderByDescending(x => x.NumberOfFreeFolders)
            .PaginatedListAsync(request.Page, request.Size);
        return result;
    }
} 