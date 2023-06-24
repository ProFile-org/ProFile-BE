namespace Application.Common.Messages;

public static class UserLogMessages
{
    public const string Add = "Added user {Username} with email {Email} of role {Role}";
    public const string Update = "Updated user";
    public const string Disable = "Disabled user";

    public static class Staff
    {
        public const string AddStaff = "Added a new staff";
        public static string AssignStaff(string roomId) => $"Assigned user to be staff of room {roomId}";
        public const string RemoveFromRoom = "Removed staff from room";
        public const string Remove = "Removed staff";
    }
}