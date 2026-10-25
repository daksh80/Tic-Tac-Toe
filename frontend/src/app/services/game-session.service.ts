import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map, of, switchMap } from 'rxjs';
import {
  MAX_GRID_SIZE,
  MIN_GRID_SIZE,
  GameMode,
  GameSnapshot,
  GameUpdate,
  Scoreboard
} from '../constants/constant';
import { GameApiService } from './game-api.service';

@Injectable({ providedIn: 'root' })
export class GameSessionService {
  /** Provides game state operations backed by the game API. */
  constructor(private readonly gameApi: GameApiService) {}

  /** Starts a game and verifies that the API created the requested board. */
  public startGame(mode: GameMode, size: number): Observable<GameUpdate> {
    return this.updateGame(this.gameApi.resetGame(mode, size, size), size);
  }

  /** Applies a move and refreshes the scoreboard if the game ends. */
  public playMove(index: number): Observable<GameUpdate> {
    return this.updateGame(this.gameApi.playMove(index));
  }

  /** Undoes the last move and refreshes scores when undoing a finished game. */
  public undoMove(game: GameSnapshot): Observable<GameUpdate> {
    return this.updateGame(
      this.gameApi.undoMove(),
      undefined,
      game.status !== 'inProgress'
    );
  }

  /** Checks whether a grid size is inside the supported range. */
  public isValidGridSize(size: number | null): size is number {
    return size !== null && Number.isInteger(size) && size >= MIN_GRID_SIZE && size <= MAX_GRID_SIZE;
  }

  /** Validates an API game response and includes updated scores when needed. */
  private updateGame(
    request: Observable<GameSnapshot>,
    expectedSize?: number,
    refreshScoreboard = false
  ): Observable<GameUpdate> {
    return request.pipe(
      switchMap((game) => {
        if (expectedSize !== undefined && (game.rows !== expectedSize || game.columns !== expectedSize)) {
          throw new Error(
            `The backend returned a ${game.rows} × ${game.columns} grid instead of ${expectedSize} × ${expectedSize}. Restart the .NET backend and try again.`
          );
        }
        if (refreshScoreboard || game.status !== 'inProgress') {
          return this.gameApi.getScoreboard().pipe(map((scoreboard) => ({ game, scoreboard })));
        }
        return of({ game });
      })
    );
  }

  /** Loads the current session scoreboard. */
  public getScoreboard(): Observable<Scoreboard> {
    return this.gameApi.getScoreboard();
  }

  /** Resets the session scoreboard through the API. */
  public resetScoreboard(): Observable<Scoreboard> {
    return this.gameApi.resetScoreboard();
  }

  /** Converts API and runtime errors into messages suitable for the UI. */
  public getErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      const detail = error.error?.detail;
      if (typeof detail === 'string') return detail;
      return `Could not reach the game API (${error.status || 'network error'}). Make sure the .NET backend is running at http://localhost:5147.`;
    }
    return error instanceof Error ? error.message : 'An unexpected error occurred while communicating with the game API.';
  }
}
