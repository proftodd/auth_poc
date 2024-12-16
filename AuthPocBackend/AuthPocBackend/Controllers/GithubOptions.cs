namespace AuthPocBackend.Controllers;

public record GithubOptions
{
    public const string Github = "GithubOptions";
    public required string AppName { get; init; }
    public required string AuthUrl { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RedirectUri { get; init; }
    public required string AllowSignup { get; init; }
}