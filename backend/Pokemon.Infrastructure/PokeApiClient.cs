using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Pokemon.Domain;
using PokemonModel = Pokemon.Domain.Pokemon;

namespace Pokemon.Infrastructure;

public sealed class PokeApiClient(HttpClient httpClient) : IPokemonClient
{
    public async Task<PokemonListResponse> GetPokemonAsync(CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync("pokemon", cancellationToken);
        response.EnsureSuccessStatusCode();

        var upstream = await response.Content.ReadFromJsonAsync<PokeApiListResponse>(
            cancellationToken: cancellationToken);
        if (upstream?.Results is null)
        {
            throw new JsonException("PokéAPI response is missing its results array.");
        }

        var items = upstream.Results.Select(resource =>
        {
            if (string.IsNullOrWhiteSpace(resource.Name))
            {
                throw new JsonException("PokéAPI returned a Pokémon without a name.");
            }

            return new PokemonModel(ParseId(resource.Url), resource.Name);
        }).ToArray();

        return new PokemonListResponse(items, upstream.Count);
    }

    private static int? ParseId(string? resourceUrl)
    {
        if (!Uri.TryCreate(resourceUrl, UriKind.Absolute, out var uri))
        {
            return null;
        }

        var lastSegment = uri.AbsolutePath.TrimEnd('/').Split('/').LastOrDefault();
        return int.TryParse(lastSegment, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0
            ? id
            : null;
    }

    private sealed class PokeApiListResponse
    {
        public int Count { get; init; }
        public List<PokemonResource>? Results { get; init; }
    }

    private sealed class PokemonResource
    {
        public string? Name { get; init; }
        public string? Url { get; init; }
    }
}
