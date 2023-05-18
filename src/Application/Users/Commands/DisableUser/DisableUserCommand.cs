using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using MediatR;

namespace Application.Users.Commands.DisableUser;

public record DisableUserCommand : IRequest<UserDto>
{
    public Guid Id { get; init; }
}

public class DisableUserCommandHandler : IRequestHandler<DisableUserCommand, UserDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public DisableUserCommandHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(DisableUserCommand request, CancellationToken cancellationToken)
    {
        var id = request.Id;
        var user = await _uow.UserRepository.GetUserByIdAsync(id);

        if (user is null)
        {
            throw new KeyNotFoundException("User does not exist");
        }
        
        var result = await _uow.UserRepository.DisableUserById(id);
        await _uow.Commit();
        return _mapper.Map<UserDto>(result);
    }
}