using System.Collections.ObjectModel;
using Application.Common.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Users.Queries.GetUsersByName;

public record GetUsersByNameQuery : IRequest<IEnumerable<UserDto>>
{
    public string FirstName { get; init; }
}

public class GetUsersByNameQueryHandler : IRequestHandler<GetUsersByNameQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetUsersByNameQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUsersByNameQuery request, CancellationToken cancellationToken)
    {
        var result = await _uow.UserRepository.GetUsersByNameAsync(request.FirstName);
        return new ReadOnlyCollection<UserDto>(_mapper.Map<List<UserDto>>(result)
                                                    .ToList());
    }
}