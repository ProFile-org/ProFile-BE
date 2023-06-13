namespace Application.Common.Messages;

public static class UserLogMessages
{
    public const string Add = "Added user";
    public const string Update = "Updated user";
    public const string Disable = "Disabled user";

    public static class Staff
    {
        public static string AddStaff(string roomId) => $"Assigned user to be staff of room {roomId}";
        public const string RemoveFromRoom = "Removed staff from room";
        public const string Remove = "Removed staff";
    }
}