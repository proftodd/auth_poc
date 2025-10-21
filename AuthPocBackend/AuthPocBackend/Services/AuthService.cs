using AuthPocBackend.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;

namespace AuthPocBackend.Services;

public interface IAuthService
{
    public string GetLoginRedirectUrl();

    public Task<(Models.User user, string jwt)> HandleCallbackAsync(string code, string state);

}

public class AuthService(IOptions<GithubOptions> github, IOptions<JwtTokenOptions> jwt, ILogger<AuthService> logger, IHttpClientFactory httpClientFactory) : IAuthService
{
    private const string State = "abc123";
    private readonly GithubOptions _github = github.Value;
    private readonly JwtTokenOptions _jwt = jwt.Value;
    private readonly ILogger<AuthService> _logger = logger;
    private readonly HttpClient _http = httpClientFactory.CreateClient();

    public string GetLoginRedirectUrl()
    {
        var q = HttpUtility.ParseQueryString(string.Empty);
        q.Add("client_id", _github.ClientId);
        q.Add("redirect_uri", _github.RedirectUri);
        q.Add("state", State);
        q.Add("allow_signup", "false");

        return $"{_github.AuthUrl}/authorize?{q}";
    }

    public async Task<(Models.User user, string jwt)> HandleCallbackAsync(string code, string state)
    {
        if (state != State)
        {
            throw new UnauthorizedAccessException("State mismatch");
        }

        var data = new Dictionary<string, string>
        {
            ["client_id"] = _github.ClientId,
            ["client_secret"] = _github.ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = _github.RedirectUri,
        };

        using var response = await _http.PostAsync($"{_github.AuthUrl}/access_token", new FormUrlEncodedContent(data));
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var queryParams = HttpUtility.ParseQueryString(responseBody);
        var accessToken = queryParams["access_token"];

        if (string.IsNullOrEmpty(accessToken))
        {
            throw new UnauthorizedAccessException("Failed to retrieve Github token");
        }

        using var userRequest = new HttpRequestMessage(HttpMethod.Get, $"{_github.ApiUrl}/user");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        userRequest.Headers.Add("Accept", "application/vnd.github+json");
        userRequest.Headers.Add("User-Agent", "Auth POC Backend");
        userRequest.Headers.Add("X-Github-Api-Version", "2022-11-28");

        using var userResponse = await _http.SendAsync(userRequest);
        userResponse.EnsureSuccessStatusCode();

        var user = await userResponse.Content.ReadFromJsonAsync<Models.User>();
        if (user == null)
        {
            throw new InvalidOperationException("Failed to parse Github user.");
        }

        var jwt = GenerateJwt(user);
        return (user, jwt);
    }

    private string GenerateJwt(Models.User user)
    {
        var cert = GetCertificateFromStore(_jwt.Thumbprint);
        if (cert == null)
        {
            throw new InvalidOperationException("Signing certificate not found.");
        }

        var key = new X509SecurityKey(cert);
        var creds = new SigningCredentials(key, SecurityAlgorithms.RsaSha256Signature);

        var claims = new[]
        {
            new Claim("LE-User-Name", user.Name ?? string.Empty),
            new Claim("LE-User-Login", user.Login ?? string.Empty),
            new Claim("LE-Company", user.Company ?? string.Empty),
        };

        var token = new JwtSecurityToken(
            _jwt.Issuer,
            _jwt.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: creds
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static X509Certificate2? GetCertificateFromStore(string thumbprint, StoreName storeName = StoreName.My)
    {
        using var certStore = new X509Store(storeName, StoreLocation.LocalMachine);
        certStore.Open(OpenFlags.ReadOnly);
        var certs = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

        if (certs.Count == 0)
        {
            // this is for local testing. I'm guessing there is a better way to do this?
            using var userStore = new X509Store(storeName, StoreLocation.CurrentUser);
            userStore.Open(OpenFlags.ReadOnly);
            certs = userStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
        }

        return certs.Count == 0 ? null : certs[0];
    }
}
