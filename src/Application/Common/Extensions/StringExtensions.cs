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
}