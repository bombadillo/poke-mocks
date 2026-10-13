using System.Text.Json;
using Pokemon.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<IPokemonClient, PokeApiClient>(client =>
{
    client.BaseAddress = new Uri("https://pokeapi.co/api/v2/");
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.MapGet("/api/pokemon", async (IPokemonClient pokemonClient, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await pokemonClient.GetPokemonAsync(cancellationToken));
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            title: "PokéAPI is unavailable.",
            statusCode: StatusCodes.Status502BadGateway);
    }
    catch (JsonException)
    {
        return Results.Problem(
            title: "PokéAPI returned an invalid response.",
            statusCode: StatusCodes.Status502BadGateway);
    }
    catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        return Results.Problem(
            title: "PokéAPI request timed out.",
            statusCode: StatusCodes.Status504GatewayTimeout);
    }
});

app.Run();
