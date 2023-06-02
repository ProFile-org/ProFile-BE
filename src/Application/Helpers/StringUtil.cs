using System.Text;

namespace Application.Helpers;

public class StringUtil
{
    private static string alphanumeric = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public static string RandomString(int n)
    {
        var stringBuilder = new StringBuilder();
        var length = alphanumeric.Length;
        for (var i = 0; i < n; i++)
        {
            stringBuilder.Append(alphanumeric.ElementAt(new Random().Next(0, length)));
        }

        return stringBuilder.ToString();
    }
}