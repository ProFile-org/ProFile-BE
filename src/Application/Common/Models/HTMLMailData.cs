using System.Text.Json.Serialization;

namespace Application.Common.Models;

public class HTMLMailData
{
    [JsonPropertyName("from")]
    public From From { get; set; }
    [JsonPropertyName("to")]
    public To[] To { get; set; }
    [JsonPropertyName("template_uuid")]
    public string TemplateUuid { get; set; }
    [JsonPropertyName("template_variables")]
    public TemplateVariables TemplateVariables { get; set; }
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

public class TemplateVariables
{
    [JsonPropertyName("user_email")]
    public string UserEmail { get; set; }
    [JsonPropertyName("pass_reset_link")]
    public string PassResetLink { get; set; }
    [JsonPropertyName("user_password")]
    public string UserPassword { get; set; }
}