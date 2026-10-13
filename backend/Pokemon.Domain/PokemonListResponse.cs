namespace Pokemon.Domain;

public sealed record PokemonListResponse(IReadOnlyList<Pokemon> Items, int Count);
