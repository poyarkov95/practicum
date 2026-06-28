namespace Infrastructure.Settings;

public class TokenMetadata
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int LifeTimeInMinutes { get; set; }
}