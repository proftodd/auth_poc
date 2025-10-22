using AuthPocBackend.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AuthPocBackend.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapGet("/login", ([FromServices] IAuthService auth) =>
        {
            var url = auth.GetLoginRedirectUrl();
            return Results.Redirect(url);
        });

        group.MapGet("/callback", async (
            [FromQuery] string code,
            [FromQuery] string state,
            [FromServices] IAuthService auth,
            HttpContext ctx) =>
        {
            var (user, jwt) = await auth.HandleCallbackAsync(code, state);
            var userJson = JsonSerializer.Serialize(user);
            return Results.Content($@"
                <html>
                    <body>
                        <script>
                            window.opener.postMessage({{
                                user: {userJson},
                                jwt: '{jwt}'
                            }}, 'http://localhost:5173')
                            console.log('postMessage sent!')
                        </script>
                    </body>
                </html>
            ", "text/html");
        });
    }
}
