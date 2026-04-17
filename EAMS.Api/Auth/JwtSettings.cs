namespace EAMS.Api.Auth;

/// <summary>
/// JWT 配置
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "EAMS";
    public string Audience { get; set; } = "EAMS.Client";
    public int ExpirationHours { get; set; } = 24;
}
