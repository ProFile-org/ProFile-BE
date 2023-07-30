using System.Text.Json.Serialization;

namespace Application.Common.Models;

public class HtmlMailData
{
    [JsonPropertyName("from")]
    public From From { get; set; }
    [JsonPropertyName("to")]
    public To[] To { get; set; }
    [JsonPropertyName("template_uuid")]
    public string TemplateUuid { get; set; }
    [JsonPropertyName("template_variables")]
    public object TemplateVariables { get; set; }
}

public class From
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
}

public class To
{
    [JsonPropertyName("email")]
    public string Email { get; set; }
}

public class ResetPasswordTemplateVariables
{
    [JsonPropertyName("user_email")]
    public string UserEmail { get; set; }
    [JsonPropertyName("token")]
    public string Token { get; set; }
    [JsonPropertyName("user_password")]
    public string UserPassword { get; set; }
}

public class ShareEntryTemplateVariables
{
    [JsonPropertyName("entry_type")]
    public string EntryType { get; set; }
    [JsonPropertyName("entry_name")]
    public string EntryName { get; set; }
    [JsonPropertyName("sharer_name")]
    public string SharerName { get; set; }
    [JsonPropertyName("operation")]
    public string Operation { get; set; }
    [JsonPropertyName("owner_name")]
    public string OwnerName { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }
}

public class CreateRequestTemplateVariables
{
    [JsonPropertyName("user_name")]
    public string UserName { get; set; }
    [JsonPropertyName("request_type")]
    public string RequestType { get; set; }
    [JsonPropertyName("operation")]
    public string Operation { get; set; }
    [JsonPropertyName("document_name")]
    public string DocumentName { get; set; }
    [JsonPropertyName("reason")]
    public string Reason { get; set; }
    [JsonPropertyName("path")]
    public string Path { get; set; }
    [JsonPropertyName("id")]
    public string Id { get; set; }
}