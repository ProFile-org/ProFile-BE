namespace Infrastructure.Shared;

public class JweSettings
{
    public string SigningKeyId { get; set; }
    public string EncryptionKeyId { get; set; }
    public TimeSpan TokenLifetime { get; set; }
    public int RefreshTokenLifetimeInDays { get; set; }
}