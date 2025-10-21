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
            ctx.Session.SetString("Jwt", jwt);
            ctx.Session.SetString("User", JsonSerializer.Serialize(user));
            return Results.Json(new { user, jwt });
        });

        group.MapGet("/jwt", (HttpContext ctx) =>
        {
            var jwt = ctx.Session.GetString("Jwt");
            var userJson = ctx.Session.GetString("User");
            if (jwt == null || userJson == null)
            {
                return Results.Unauthorized();
            }
            return Results.Ok(new { user = userJson, jwt });
        });
    }
}
