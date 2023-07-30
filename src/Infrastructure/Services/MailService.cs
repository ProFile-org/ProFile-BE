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

    public bool SendResetPasswordHtmlMail(string userEmail, string temporaryPassword, string token)
    {
        var data = new HtmlMailData()
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
            TemplateUuid = _mailSettings.TemplateUuids.ResetPassword,
            TemplateVariables = new ResetPasswordTemplateVariables()
            {
                UserEmail = userEmail,
                UserPassword = temporaryPassword,
                Token = token
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

    public bool SendShareEntryHtmlMail(bool isDirectory, string name, string sharerName, string operation, string ownerName,
        string email, string path)
    {
        var data = new HtmlMailData()
        {
            From = new From()
            {
                Email = _mailSettings.SenderEmail,
                Name = _mailSettings.SenderName,
            },
            To = new To[]
            {
                new (){ Email = email },
            },
            TemplateUuid = _mailSettings.TemplateUuids.ShareEntry,
            TemplateVariables = new ShareEntryTemplateVariables()
            {
                EntryName = name,
                Operation = operation,
                EntryType = isDirectory ? "Folder" : "File",
                OwnerName = ownerName,
                SharerName = sharerName,
                Path = path
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

    public bool SendCreateRequestHtmlMail(string userName, string requestType, string operation, string documentName,
        string reason, Guid documentId, string email)
    {
        var data = new HtmlMailData()
        {
            From = new From()
            {
                Email = _mailSettings.SenderEmail,
                Name = _mailSettings.SenderName,
            },
            To = new To[]
            {
                new (){ Email = email },
            },
            TemplateUuid = _mailSettings.TemplateUuids.Request,
            TemplateVariables = new CreateRequestTemplateVariables()
            {
                Operation = operation,
                Reason = reason,
                DocumentName = documentName,
                UserName = userName,
                Id = documentId.ToString(),
                RequestType = requestType,
                Path = !requestType.Equals("borrow request") ? "import/manage" : "requests"
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