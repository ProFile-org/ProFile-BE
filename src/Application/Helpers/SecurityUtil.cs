namespace Application.Helpers;

using System.Security.Cryptography;
using System.Text;

public static class SecurityUtil
{
    public static string Hash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        var stringBuilder = new StringBuilder();
            
        foreach (var b in hash)
        {
            stringBuilder.Append(b.ToString("x2"));
        }
            
        return stringBuilder.ToString();
    }

    public static string HashPasswordWith(this string input, string salt, string pepper)
    {
        pepper = Convert.ToBase64String(Encoding.UTF8.GetBytes(pepper)); 
        salt = Convert.ToBase64String(Encoding.UTF8.GetBytes(salt));
        return Hash(salt + input + pepper);
    }
}