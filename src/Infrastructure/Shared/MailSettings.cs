namespace Infrastructure.Shared;

public class MailSettings
{
    public string ClientUrl { get; set; }
    public string Token { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    
    public Template TemplateUuids { get; set; }
}

public class Template
{
    public string ResetPassword { get; set; }
    public string ShareEntry { get; set; }
    public string Request { get; set; }
}