import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { PokemonListResponse } from './pokemon.models';

@Injectable({ providedIn: 'root' })
export class PokemonApiService {
  private readonly http = inject(HttpClient);

  getPokemon(): Observable<PokemonListResponse> {
    return this.http.get<PokemonListResponse>('/api/pokemon');
  }
}
