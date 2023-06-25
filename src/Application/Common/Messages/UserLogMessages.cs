namespace Application.Common.Messages;

public static class UserLogMessages
{
    public const string Add = "Added user {Username} with email {Email} of role {Role}";
    public const string Update = "Updated user {Username} information";
    public const string Disable = "Disabled user";

    public static class Staff
    {
        public const string AddStaff = "Add a new staff with id {StaffId}";
        public const string AssignStaff = "Assign staff {StaffId} to room {RoomId}";
        public const string RemoveFromRoom = "Remove staff {StaffId} from room {RoomId}";
        public const string Remove = "Removed staff";
    }
}