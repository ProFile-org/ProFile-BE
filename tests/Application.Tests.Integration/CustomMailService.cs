using Application.Common.Interfaces;

namespace Application.Tests.Integration;

public class CustomMailService : IMailService
{
    public bool SendResetPasswordHtmlMail(string userEmail, string password)
    {
        return true;
    }
}