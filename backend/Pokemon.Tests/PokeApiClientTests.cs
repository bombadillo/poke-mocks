using System.Net;
using System.Text;
using Pokemon.Infrastructure;

namespace Pokemon.Tests;

public sealed class PokeApiClientTests
{
    [Fact]
    public async Task GetPokemonAsync_maps_names_ids_and_upstream_total()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, FixtureLoader.Read("initial-load"));
        using var httpClient = CreateClient(handler);
        var client = new PokeApiClient(httpClient);

        var result = await client.GetPokemonAsync(CancellationToken.None);

        Assert.Equal(1302, result.Count);
        Assert.Collection(
            result.Items,
            pokemon =>
            {
                Assert.Equal(1, pokemon.Id);
                Assert.Equal("bulbasaur", pokemon.Name);
            },
            pokemon =>
            {
                Assert.Equal(2, pokemon.Id);
                Assert.Equal("ivysaur", pokemon.Name);
            },
            pokemon =>
            {
                Assert.Equal(3, pokemon.Id);
                Assert.Equal("venusaur", pokemon.Name);
            });
    }

    [Fact]
    public async Task GetPokemonAsync_accepts_empty_results()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, FixtureLoader.Read("empty-results"));
        using var httpClient = CreateClient(handler);
        var client = new PokeApiClient(httpClient);

        var result = await client.GetPokemonAsync(CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public async Task GetPokemonAsync_throws_for_malformed_json()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, "{not-json");
        using var httpClient = CreateClient(handler);
        var client = new PokeApiClient(httpClient);

        await Assert.ThrowsAsync<System.Text.Json.JsonException>(
            () => client.GetPokemonAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetPokemonAsync_throws_for_upstream_http_errors()
    {
        using var handler = new StubHttpMessageHandler(
            HttpStatusCode.ServiceUnavailable,
            FixtureLoader.Read("api-error"));
        using var httpClient = CreateClient(handler);
        var client = new PokeApiClient(httpClient);

        await Assert.ThrowsAsync<HttpRequestException>(
            () => client.GetPokemonAsync(CancellationToken.None));
    }

    [Fact]
    public async Task GetPokemonAsync_requests_the_default_first_page()
    {
        using var handler = new StubHttpMessageHandler(HttpStatusCode.OK, FixtureLoader.Read("initial-load"));
        using var httpClient = CreateClient(handler);
        var client = new PokeApiClient(httpClient);

        await client.GetPokemonAsync(CancellationToken.None);

        Assert.NotNull(handler.RequestUri);
        Assert.Equal("/api/v2/pokemon", handler.RequestUri!.AbsolutePath);
        Assert.Equal(string.Empty, handler.RequestUri.Query);
    }

    private static HttpClient CreateClient(HttpMessageHandler handler) =>
        new(handler) { BaseAddress = new Uri("https://pokeapi.co/api/v2/") };

    private sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
