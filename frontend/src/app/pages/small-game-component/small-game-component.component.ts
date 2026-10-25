import { Component } from '@angular/core';
import { NgSwitch, NgSwitchCase, NgSwitchDefault } from '@angular/common';
import { GameListComponent } from '../../components/game-list/game-list.component';
import { TicTacToeComponent } from '../../components/tic-tac-toe/tic-tac-toe.component';
import { GAME_LIST, Game } from '../../constants/constant';

@Component({
  selector: 'app-small-game-component',
  standalone: true,
  imports: [NgSwitch, NgSwitchCase, NgSwitchDefault, GameListComponent, TicTacToeComponent],
  templateUrl: './small-game-component.component.html',
  styleUrl: './small-game-component.component.css'
})
export class SmallGameComponentComponent {
  readonly gameList = GAME_LIST;
  selectedGame: Game | null = null;

  /** Updates the studio view to show the selected game. */
  public onGameSelected(game: Game): void {
    this.selectedGame = game;
  }
}
