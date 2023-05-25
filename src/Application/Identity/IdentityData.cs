namespace Application.Identity;

public static class IdentityData
{
    public static class Claims
    {
        public const string Role = "role";
        public const string Department = "department";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Staff = "Staff";
    }
}