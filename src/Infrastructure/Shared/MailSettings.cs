namespace Infrastructure.Shared;

public class MailSettings
{
    public string ClientUrl { get; set; }
    public string Token { get; set; }
    public string SenderName { get; set; }
    public string SenderEmail { get; set; }
    public string TemplateUuid { get; set; }
}