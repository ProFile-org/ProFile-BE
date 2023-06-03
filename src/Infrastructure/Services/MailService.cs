using System.Text.Json;
using Application.Common.Interfaces;
using Application.Common.Models;
using Infrastructure.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using RestSharp;

namespace Infrastructure.Services;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;
    
    public MailService(IOptions<MailSettings> mailSettingsOptions)
    {
        _mailSettings = mailSettingsOptions.Value;
    }

    public bool SendResetPasswordHtmlMail(string userEmail, string password)
    {
        var data = new HTMLMailData()
        {
            From = new From()
            {
                Email = _mailSettings.SenderEmail,
                Name = _mailSettings.SenderName,
            },
            To = new To[]
            {
                new (){ Email = userEmail },
            },
            TemplateUuid = _mailSettings.TemplateUuid,
            TemplateVariables = new TemplateVariables()
            {
                UserEmail = userEmail,
                PassResetLink = "random_shit",
                UserPassword = password,
            },
        };
        
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        
        var client = new RestClient(_mailSettings.ClientUrl);
        var request = new RestRequest
        {
            Method = Method.Post
        };
        
        request.AddHeader("Authorization", $"{JwtBearerDefaults.AuthenticationScheme} {_mailSettings.Token}");
        request.AddHeader("Content-Type", "application/json");
        request.AddParameter("application/json", json, ParameterType.RequestBody);
        var response = client.Execute(request);
        return response.IsSuccessStatusCode;
    }
}