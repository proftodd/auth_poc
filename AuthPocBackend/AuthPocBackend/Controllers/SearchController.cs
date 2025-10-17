using AuthPocBackend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthPocBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(ILogger<SearchController> logger) : ControllerBase
{
    private readonly ILogger<SearchController> _logger = logger;

    [HttpGet]
    [Authorize(Policy = "IsUser")]
    public IActionResult Search([FromQuery(Name="st")] string[] searchTerms) =>
        Ok(
            new SearchResult
            {
                SearchTerms = searchTerms,
                Entities = [
                    new() {
                        Id = 1,
                        Substance = new() {
                            Id = 1,
                            InchiKey = "XLYOFNOQVPJJNP-UHFFFAOYSA-N",
                            Inchi = "InChI=1S/H2O/h1H2",
                        },
                        Num9000 = new() { Id = 1, Num = 9000 },
                        Descriptors = [
                            new() { Id = 1, Desc = "Water", },
                            new() { Id = 2, Desc = "H2O", },
                        ],
                    }
                ],
            }
        );
}
