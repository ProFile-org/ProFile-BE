using Application.Identity;

namespace Application.Common.Extensions;

public static class StringExtensions
{
    public static bool MatchesPropertyName<T>(this string input)
        where T : class
    {
        var type = typeof(T);
        var properties = type.GetProperties();

        return properties.Any(property => string.Equals(property.Name, input));
    }

    public static bool IsAdmin(this string role)
        => role.Equals(IdentityData.Roles.Admin);
    
    public static bool IsStaff(this string role)
        => role.Equals(IdentityData.Roles.Staff);
    
    public static bool IsEmployee(this string role)
        => role.Equals(IdentityData.Roles.Employee);
}