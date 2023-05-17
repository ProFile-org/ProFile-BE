using Application.Common.Interfaces;
using AutoMapper;
using MediatR;

namespace Application.Users.Queries;

public record GetUserByNameQuery : IRequest<IEnumerable<UserDto>>
{
    public string FirstName { get; init; }
}

public class GetUserByNameQueryHandler : IRequestHandler<GetUserByNameQuery, IEnumerable<UserDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public GetUserByNameQueryHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> Handle(GetUserByNameQuery request, CancellationToken cancellationToken)
    {
        var result = await _uow.UserRepository.GetUserByNameAsync(request.FirstName);
        return _mapper.Map<IEnumerable<UserDto>>(result);
    }
}