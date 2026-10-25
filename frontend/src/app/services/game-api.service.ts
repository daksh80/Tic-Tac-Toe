import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { GAME_API_URL, GameMode, GameResponse, GameSnapshot, Scoreboard } from '../constants/constant';

@Injectable({ providedIn: 'root' })
export class GameApiService {
  /** Provides HTTP access to the game API. */
  constructor(private readonly http: HttpClient) {}

  /** Starts a new game with the selected mode and board dimensions. */
  public resetGame(mode: GameMode, rows: number, columns: number): Observable<GameSnapshot> {
    return this.http
      .post<GameResponse>(`${GAME_API_URL}/game/reset`, {
        mode,
        rows,
        columns,
        boardSize: rows === columns ? rows : undefined
      })
      .pipe(map(normalizeGame));
  }

  /** Sends a move to the game API and normalizes the returned board. */
  public playMove(cellIndex: number): Observable<GameSnapshot> {
    return this.http.post<GameResponse>(`${GAME_API_URL}/game/moves`, { cellIndex }).pipe(map(normalizeGame));
  }

  /** Asks the API to undo the most recent move. */
  public undoMove(): Observable<GameSnapshot> {
    return this.http.post<GameResponse>(`${GAME_API_URL}/game/undo`, {}).pipe(map(normalizeGame));
  }

  /** Retrieves the current session scoreboard. */
  public getScoreboard(): Observable<Scoreboard> {
    return this.http.get<Scoreboard>(`${GAME_API_URL}/scoreboard`);
  }

  /** Clears all scores in the current session. */
  public resetScoreboard(): Observable<Scoreboard> {
    return this.http.post<Scoreboard>(`${GAME_API_URL}/scoreboard/reset`, {});
  }
}

/** Adds missing legacy dimensions and validates the returned board size. */
function normalizeGame(game: GameResponse): GameSnapshot {
  const legacySize = game.boardSize ?? Math.sqrt(game.board.length);
  const rows = game.rows ?? legacySize;
  const columns = game.columns ?? legacySize;

  if (
    !Number.isInteger(rows) ||
    !Number.isInteger(columns) ||
    rows < 1 ||
    columns < 1 ||
    rows * columns !== game.board.length
  ) {
    throw new Error('The game server returned board dimensions that do not match its cells.');
  }

  return { ...game, rows, columns };
}
