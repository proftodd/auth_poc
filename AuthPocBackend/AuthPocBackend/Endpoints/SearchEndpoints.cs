using AuthPocBackend.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthPocBackend.Endpoints;

public static class SearchEndpoints
{

    public static void MapSearchEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/search", ([FromServices] ISearchService searchService, [FromQuery(Name = "st")] string[] searchTerms) =>
            Results.Ok(searchService.Search(searchTerms)));
    }
}
