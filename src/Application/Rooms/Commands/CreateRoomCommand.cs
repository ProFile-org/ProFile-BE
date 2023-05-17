using Application.Common.Interfaces;
using Application.Users.Queries;
using AutoMapper;
using Domain.Entities.Physical;
using MediatR;

namespace Application.Rooms.Commands;

public record CreateRoomCommand : IRequest<RoomDto>
{
    public string Name { get; init; }
    public Guid StaffId { get; init; }
    public string Description { get; init; }
    public int NumberOfLockers { get; init; }
}

public class CreateRoomCommandHandler : IRequestHandler<CreateRoomCommand, RoomDto>
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CreateRoomCommandHandler(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }
    
    public async Task<RoomDto> Handle(CreateRoomCommand request, CancellationToken cancellationToken)
    {
        var staff = await _uow.StaffRepository.GetByIdAsync(request.StaffId);
        if (staff is null)
        {
            throw new KeyNotFoundException($"Staff with id:{request.StaffId} does not exist");
        }

        var entity = new Room
        {
            Staff = staff,
            Name = request.Name,
            Description = request.Description,
            NumberOfLockers = request.NumberOfLockers
        };
        var result = await _uow.RoomRepository.AddRoomAsync(entity);
        await _uow.Commit();
        return _mapper.Map<RoomDto>(result);
    }
}