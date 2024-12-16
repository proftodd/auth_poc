using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AuthPocBackend.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class AuthController(ILogger<AuthController> logger, IOptions<GithubOptions> options) : ControllerBase
{
    private static readonly HttpClient _client = new();
    private readonly ILogger<AuthController> _logger = logger;
    private readonly GithubOptions _options = options.Value;

    [HttpGet]
    public async Task<IActionResult> Login(string code, string state)
    {
        _logger.LogInformation("Redirect from Github received: code = [{Code}]", code);
        var data = new Dictionary<string, string>(
            [
                new KeyValuePair<string, string>("client_id", _options.ClientId),
                new KeyValuePair<string, string>("client_secret", _options.ClientSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("redirect_uri", _options.RedirectUri),
            ]
        );
        using var response = await _client.PostAsync(_options.AuthUrl + "/access_token", JsonContent.Create(data));
        return response.IsSuccessStatusCode
            ? new OkObjectResult(await response.Content.ReadAsStringAsync())
            : new EmptyResult()
        ;
    }
}