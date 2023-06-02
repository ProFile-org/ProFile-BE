using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface IMailService
{
    bool SendResetPasswordHtmlMail(string userEmail, string password);
}