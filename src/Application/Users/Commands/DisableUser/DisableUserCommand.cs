using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Users.Commands.DisableUser;

public record DisableUserCommand : IRequest<UserDto>
{
    public Guid Id { get; init; }
}

public class DisableUserCommandHandler : IRequestHandler<DisableUserCommand, UserDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public DisableUserCommandHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(DisableUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException("User does not exist");
        }

        user.IsActive = false;

        var result = _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<UserDto>(result);
    }
}