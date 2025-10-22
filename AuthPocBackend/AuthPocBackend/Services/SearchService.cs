using AuthPocBackend.Models;

namespace AuthPocBackend.Services;

public interface ISearchService
{
    public SearchResult Search(string[] searchTerms);
}
public class SearchService : ISearchService
{
    public SearchResult Search(string[] searchTerms) =>
        new()
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
        };
}
