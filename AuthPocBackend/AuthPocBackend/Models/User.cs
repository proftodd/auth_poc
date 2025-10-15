namespace AuthPocBackend.Models;

public record User
{
    public string Name { get; init; } = string.Empty;
    public string Login { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
    public bool SiteAdmin { get; init; } = false;
    public string Company { get; init; } = string.Empty;
    public string OrganizationsUrl { get; init; } = string.Empty;
    public string Jwt { get; internal set; } = string.Empty;
}
