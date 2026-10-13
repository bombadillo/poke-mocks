import { Component, OnInit, inject, signal } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { PokemonApiService } from './pokemon-api.service';
import { Pokemon, PokemonListResponse } from './pokemon.models';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './app.component.html'
})
export class AppComponent implements OnInit {
  private readonly pokemonApi = inject(PokemonApiService);

  readonly pokemon = signal<Pokemon[]>([]);
  readonly count = signal(0);
  readonly loading = signal(true);
  readonly error = signal(false);

  ngOnInit(): void {
    this.pokemonApi.getPokemon().subscribe({
      next: (response: PokemonListResponse) => {
        this.pokemon.set(response.items);
        this.count.set(response.count);
        this.loading.set(false);
      },
      error: () => {
        this.error.set(true);
        this.loading.set(false);
      }
    });
  }
}
