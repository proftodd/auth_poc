using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AuthPocBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private const string State = "abc123";
    private readonly GithubOptions _options;
    private readonly JwtTokenOptions _jwtTokenOptions;
    private readonly SigningCredentials _credentials;
    private readonly ILogger<AuthController> _logger;
    private static readonly HttpClient Client = new();

    public AuthController(ILogger<AuthController> logger, IOptions<GithubOptions> options, IOptions<JwtTokenOptions> jwtTokenOptions)
    {
        _options = options.Value;
        _jwtTokenOptions = jwtTokenOptions.Value;
        var certificate = GetCertificateFromStore(_jwtTokenOptions.Thumbprint);
        var privateKey = new X509SecurityKey(certificate);
        _credentials = new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256Signature);
        _logger = logger;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("client_id", _options.ClientId);
        queryString.Add("redirect_uri", _options.RedirectUri);
        queryString.Add("state", State);
        queryString.Add("allow_signup", "false");

        var url = $"{_options.AuthUrl}/authorize?{queryString}";
        return Redirect(url);
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback(string code, string state)
    {
        if (state != State)
        {
            _logger.LogWarning("State mismatch. Possible CSRF attach.");
            return new UnauthorizedResult();
        }
        _logger.LogInformation("Redirect from Github received: code = [{Code}] state = [{State}", code, state);
        var data = new Dictionary<string, string>(
            [
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _options.RedirectUri),
            ]
        );
        using var response = await Client.PostAsync($"{_options.AuthUrl}/access_token", new FormUrlEncodedContent(data));
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var queryParams = HttpUtility.ParseQueryString(responseString);
        var accessToken = queryParams["access_token"];

        if (string.IsNullOrEmpty(accessToken))
        {
            return Unauthorized("Failed to retrieve access token.");
        }

        using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
        userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        userRequest.Headers.Add("Accept", "application/vnd.github+json");
        userRequest.Headers.Add("User-Agent", "Auth POC Backend");
        userRequest.Headers.Add("X-Github-Api-Version", "2022-11-28");
        var userResponse = await Client.SendAsync(userRequest);
        userResponse.EnsureSuccessStatusCode();
        var user = await userResponse.Content.ReadFromJsonAsync<Models.User>();

        var jwt = GenerateToken(user);
        user.Jwt = jwt;

        HttpContext.Session.SetString("GithubToken", accessToken);
        return Redirect($"http://localhost:5173/dashboard?token={user}");
    }

    [HttpGet("token")]
    public IActionResult GetToken()
    {
        var token = HttpContext.Session.GetString("GithubToken");
        return token == null
            ? Unauthorized()
            : Ok(new { access_token = token });
    }

    private static X509Certificate2? GetCertificateFromStore(string thumbprint, StoreName storeName = StoreName.My)
    {
        X509Store certStore = new X509Store(storeName, StoreLocation.LocalMachine);
        try
        {
            certStore.Open(OpenFlags.ReadOnly);
            var certs = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

            if (certs.Count == 0)
            {
                // this is for local testing. I'm guessing there is a better way to do this?
                certStore.Close();
                certStore = new X509Store(storeName, StoreLocation.CurrentUser);
                certStore.Open(OpenFlags.ReadOnly);
                certs = certStore.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            }

            if (certs.Count == 0)
            {
                return null;
            }

            return certs[0];
        } finally
        {
            certStore.Close();
        }
    }

    private string GenerateToken(Models.User user)
    {
        var claims = new List<Claim>()
        {
            new("LE-User-Name", user.Name ?? string.Empty),
            new("LE-User-Login", user.Login ?? string.Empty),
            new("LE-Company", user.Company ?? string.Empty),
        };
        var token = new JwtSecurityToken(
            _jwtTokenOptions.Issuer,
            _jwtTokenOptions.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(1),
            signingCredentials: _credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}