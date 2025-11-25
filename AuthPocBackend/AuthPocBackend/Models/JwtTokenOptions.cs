namespace AuthPocBackend.Models;

public record JwtTokenOptions
{
    public const string Jwt = "JwtTokenOptions";
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Thumbprint { get; init; } = string.Empty;
    public string SigningCertificate { get; init; } = string.Empty;
}
