using Pokemon.Domain;

namespace Pokemon.Infrastructure;

public interface IPokemonClient
{
    Task<PokemonListResponse> GetPokemonAsync(CancellationToken cancellationToken);
}
