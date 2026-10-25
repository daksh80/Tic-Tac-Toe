import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import {
  DEFAULT_GAME_MODE,
  DEFAULT_GRID_SIZE,
  GAME_ACTIONS,
  GAME_MODES,
  MAX_GRID_SIZE,
  MIN_GRID_SIZE,
  SCOREBOARD_LABELS,
  GameAction,
  GameMode,
  GameSnapshot,
  GameUpdate,
  Scoreboard
} from '../../constants/constant';
import { GameSessionService } from '../../services/game-session.service';

@Component({
  selector: 'app-tic-tac-toe',
  standalone: true,
  templateUrl: './tic-tac-toe.component.html',
  styleUrl: './tic-tac-toe.component.css'
})
export class TicTacToeComponent implements OnInit {
  readonly modes = GAME_MODES;
  readonly scoreboardLabels = SCOREBOARD_LABELS;
  readonly gameActions = GAME_ACTIONS;
  readonly minGridSize = MIN_GRID_SIZE;
  readonly maxGridSize = MAX_GRID_SIZE;
  game: GameSnapshot | null = null;
  scoreboard: Scoreboard | null = null;
  selectedMode = DEFAULT_GAME_MODE;
  gridSize: number | null = DEFAULT_GRID_SIZE;
  loading = true;
  busy = false;
  errorMessage = '';

  /** Provides game session operations to the component. */
  constructor(private readonly gameSession: GameSessionService) {}

  /** Loads the scoreboard when the game view opens. */
  public ngOnInit(): void {
    this.gameSession.getScoreboard().subscribe({
      next: (scoreboard) => {
        this.scoreboard = scoreboard;
        this.loading = false;
      },
      error: (error: unknown) => {
        this.errorMessage = this.gameSession.getErrorMessage(error);
        this.loading = false;
      }
    });
  }

  /** Starts a fresh game using the requested mode. */
  public startGame(mode: GameMode = this.selectedMode): void {
    if (this.busy) return;
    this.selectedMode = mode;
    const size = this.game?.rows ?? this.gridSize ?? DEFAULT_GRID_SIZE;
    this.runGameRequest(this.gameSession.startGame(mode, size));
  }

  /** Supports Enter and Space activation for the mode controls. */
  public onModeKeydown(event: KeyboardEvent, mode: GameMode): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.startGame(mode);
    }
  }

  /** Runs the action selected from the game action list. */
  public runGameAction(action: GameAction): void {
    if (action === 'undo') {
      this.undoMove();
    } else {
      this.startGame();
    }
  }

  /** Validates the selected board size and creates a new game. */
  public createGrid(event: SubmitEvent): void {
    event.preventDefault();
    if (!this.gameSession.isValidGridSize(this.gridSize)) {
      this.errorMessage = 'Enter a whole number from 3 to 10.';
      return;
    }

    this.errorMessage = '';
    this.runGameRequest(
      this.gameSession.startGame(this.selectedMode, this.gridSize)
    );
  }

  /** Stores the latest value entered in the grid size field. */
  public updateGridSize(event: Event): void {
    const value = (event.target as HTMLInputElement).valueAsNumber;
    this.gridSize = Number.isNaN(value) ? null : value;
  }

  /** Sends a move when the selected cell is playable. */
  public playCell(index: number): void {
    if (this.busy || !this.game || this.game.status !== 'inProgress' || this.game.board[index] !== null) {
      return;
    }
    this.runGameRequest(this.gameSession.playMove(index));
  }

  /** Supports Enter and Space activation for board cells. */
  public onCellKeydown(event: KeyboardEvent, index: number): void {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      this.playCell(index);
    }
  }

  /** Requests that the previous move be undone. */
  public undoMove(): void {
    if (this.game?.moves.length) {
      this.runGameRequest(this.gameSession.undoMove(this.game));
    }
  }

  /** Clears the current session scores. */
  public resetScoreboard(): void {
    this.busy = true;
    this.errorMessage = '';
    this.gameSession.resetScoreboard().subscribe({
      next: (scoreboard) => {
        this.scoreboard = scoreboard;
        this.busy = false;
      },
      error: (error: unknown) => this.handleError(error)
    });
  }

  /** Checks whether a board cell belongs to the winning line. */
  public isWinningCell(index: number): boolean {
    return this.game?.winningCells.includes(index) ?? false;
  }

  /** Reports whether the current game has a move to undo. */
  public canUndo(): boolean {
    return (this.game?.moves.length ?? 0) > 0;
  }

  /** Returns the selected scoreboard value, defaulting to zero. */
  public score(key: keyof Scoreboard): number {
    return this.scoreboard?.[key] ?? 0;
  }

  /** Builds an accessible label for a board cell. */
  public cellLabel(index: number, cell: string | null): string {
    const columns = this.game?.columns ?? this.gridSize ?? DEFAULT_GRID_SIZE;
    const row = Math.floor(index / columns) + 1;
    const column = (index % columns) + 1;
    return `Row ${row}, column ${column}, ${cell ?? 'empty'}`;
  }

  /** Describes the winner, draw, or player whose turn is active. */
  public get statusMessage(): string {
    if (!this.game) {
      return '';
    }
    if (this.game.status === 'won') {
      return `Player ${this.game.winner} wins!`;
    }
    if (this.game.status === 'draw') {
      return 'The game is a draw.';
    }
    if (this.game.mode === 'computer' && this.game.currentPlayer === 'O') {
      return 'Computer is thinking...';
    }
    return `Player ${this.game.currentPlayer}'s turn`;
  }

  /** Applies a game response and updates the view state. */
  private runGameRequest(request: Observable<GameUpdate>): void {
    this.busy = true;
    this.errorMessage = '';
    request.subscribe({
      next: ({ game, scoreboard }) => {
        this.game = game;
        this.selectedMode = game.mode;
        this.gridSize = game.rows === game.columns ? game.rows : Math.max(game.rows, game.columns);
        if (scoreboard) this.scoreboard = scoreboard;
        this.busy = false;
      },
      error: (error: unknown) => this.handleError(error)
    });
  }

  /** Stores an error message and clears the pending state. */
  private handleError(error: unknown): void {
    this.errorMessage = this.gameSession.getErrorMessage(error);
    this.busy = false;
  }
}
