namespace Application.Common.Interfaces;

public interface ISecurityService
{
    string Hash(string input, string salt);
}