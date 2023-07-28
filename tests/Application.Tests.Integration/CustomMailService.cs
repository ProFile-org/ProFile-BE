using Application.Common.Interfaces;

namespace Application.Tests.Integration;

public class CustomMailService : IMailService
{
    public bool SendResetPasswordHtmlMail(string userEmail, string temporaryPassword)
    {
        return true;
    }

    public bool SendShareEntryHtmlMail(bool isDirectory, string name, string sharerName, string operation, string ownerName,
        string email, string path)
    {
        return true;
    }

    public bool SendCreateRequestHtmlMail(string userName, string requestType, string operation, string documentName,
        string reason, Guid documentId, string email)
    {
        return true;
    }
}