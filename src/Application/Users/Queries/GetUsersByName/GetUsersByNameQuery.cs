using System.Collections.Immutable;
using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Queries.GetUsersByName;

public record GetUsersByNameQuery : IRequest<IEnumerable<UserDto>>
{
    public string FirstName { get; init; }
}

public class GetUsersByNameQueryHandler : IRequestHandler<GetUsersByNameQuery, IEnumerable<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetUsersByNameQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersByNameQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Where(x => x.FirstName.Contains(request.FirstName))
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
        return new ReadOnlyCollection<UserDto>(users);
    }
}