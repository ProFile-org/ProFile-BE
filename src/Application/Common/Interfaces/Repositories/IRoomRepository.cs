using Domain.Entities.Physical;

namespace Application.Common.Interfaces.Repositories;

public interface IRoomRepository
{
    Task<Room> AddRoomAsync(Room room);
}