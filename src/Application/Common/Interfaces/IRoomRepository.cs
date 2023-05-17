using Domain.Entities.Physical;

namespace Application.Common.Interfaces;

public interface IRoomRepository
{
    Task<Room> AddRoomAsync(Room room);
}