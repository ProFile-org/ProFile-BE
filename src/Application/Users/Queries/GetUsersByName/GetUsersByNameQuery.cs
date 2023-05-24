using Application.Common.Interfaces;
using Application.Common.Mappings;
using Application.Common.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;

namespace Application.Users.Queries.GetUsersByName;

public record GetUsersByNameQuery : IRequest<PaginatedList<UserDto>>
{
    public string? SearchTerm { get; init; }
    public int? Page { get; init; }
    public int? Size { get; init; }
}

public class GetUsersByNameQueryHandler : IRequestHandler<GetUsersByNameQuery, PaginatedList<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetUsersByNameQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<UserDto>> Handle(GetUsersByNameQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.Page ?? 1;
        var sizeNumber = request.Size ?? 5;
        var users = await _context.Users
            .Where(x => string.IsNullOrEmpty(request.SearchTerm) 
                        || x.FirstName.ToLower().Contains(request.SearchTerm.ToLower())
                        || x.LastName.ToLower().Contains(request.SearchTerm.ToLower()))
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .OrderBy(x => x.Username)
            .PaginatedListAsync(pageNumber, sizeNumber);
        return users;
    }
}