export interface Pokemon {
  id: number | null;
  name: string;
}

export interface PokemonListResponse {
  items: Pokemon[];
  count: number;
}
