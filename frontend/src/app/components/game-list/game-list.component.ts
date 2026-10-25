import { Component, EventEmitter, Input, Output } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, Subject } from 'rxjs';
import { SearchComponent } from '../../common/search/search.component';
import { Game } from '../../constants/constant';

@Component({
  selector: 'app-game-list',
  standalone: true,
  imports: [SearchComponent],
  templateUrl: './game-list.component.html',
  styleUrl: './game-list.component.css'
})
export class GameListComponent {
  @Input({ required: true }) games: readonly Game[] = [];
  @Output() gameSelected = new EventEmitter<Game>();
  searchTerm = '';
  private readonly searchTerms = new Subject<string>();

  /** Sets up the debounced search subscription for this component. */
  constructor() {
    this.searchTerms
      .pipe(debounceTime(300), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((searchTerm) => (this.searchTerm = searchTerm.trim().toLowerCase()));
  }

  /** Returns games whose names match the current search text. */
  public get filteredGames(): readonly Game[] {
    return this.games.filter((game) => game.name.toLowerCase().includes(this.searchTerm));
  }

  /** Passes new search text through the debounce stream. */
  public onSearchChange(searchTerm: string): void {
    this.searchTerms.next(searchTerm);
  }

  /** Emits a selection when the requested game is available. */
  public selectGame(game: Game): void {
    if (game.isAvailable) {
      this.gameSelected.emit(game);
    }
  }
}
