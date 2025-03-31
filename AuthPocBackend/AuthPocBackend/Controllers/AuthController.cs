using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthPocBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController(ILogger<AuthController> logger, IOptions<GithubOptions> options) : ControllerBase
{
    private const string State = "abc123";
    private readonly GithubOptions _options = options.Value;
    private static readonly HttpClient Client = new();

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
            logger.LogWarning("State mismatch. Possible CSRF attach.");
            return new UnauthorizedResult();
        }
        logger.LogInformation("Redirect from Github received: code = [{Code}] state = [{State}", code, state);
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
        
        HttpContext.Session.SetString("GithubToken", accessToken);
        return Redirect($"http://localhost:5173/dashboard?token={accessToken}");
    }

    [HttpGet("token")]
    public IActionResult GetToken()
    {
        var token = HttpContext.Session.GetString("GithubToken");
        return token == null
            ? Unauthorized()
            : Ok(new { access_token = token });
    }
}