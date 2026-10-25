export interface Game {
  id: string;
  name: string;
  isAvailable: boolean;
}

export type GameMode = 'twoPlayer' | 'computer';
export type Player = 'X' | 'O';
export type GameStatus = 'inProgress' | 'won' | 'draw';
export type GameAction = 'undo' | 'reset';

export interface GameMove {
  moveNumber: number;
  player: Player;
  row: number;
  column: number;
}

export interface GameSnapshot {
  sessionId: string;
  mode: GameMode;
  rows: number;
  columns: number;
  board: (Player | null)[];
  currentPlayer: Player;
  status: GameStatus;
  winner: Player | null;
  winningCells: number[];
  moves: GameMove[];
}

export type GameResponse = Omit<GameSnapshot, 'rows' | 'columns'> & {
  rows?: number;
  columns?: number;
  boardSize?: number;
};

export interface Scoreboard {
  xWins: number;
  oWins: number;
  draws: number;
}

export interface GameUpdate {
  game: GameSnapshot;
  scoreboard?: Scoreboard;
}

export const MIN_GRID_SIZE = 3;
export const MAX_GRID_SIZE = 10;
export const DEFAULT_GRID_SIZE = 3;
export const DEFAULT_GAME_MODE: GameMode = 'twoPlayer';
export const GAME_API_URL = '/api';

export const GAME_MODES: { value: GameMode; label: string }[] = [
  { value: 'twoPlayer', label: 'Two Player' },
  { value: 'computer', label: 'Play Against Computer' }
];

export const SCOREBOARD_LABELS: { key: keyof Scoreboard; label: string }[] = [
  { key: 'xWins', label: 'X wins' },
  { key: 'oWins', label: 'O wins' },
  { key: 'draws', label: 'Draws' }
];

export const GAME_ACTIONS: { id: GameAction; label: string }[] = [
  { id: 'undo', label: 'Undo Last Move' },
  { id: 'reset', label: 'Reset Game' }
];

export const GAME_LIST: Game[] = [
  {
    id: 'tic-tac-toe',
    name: 'Tic-Tac-Toe',
    isAvailable: true
  },
  {
    id: 'connect-four',
    name: 'Connect Four',
    isAvailable: false
  },
  {
    id: 'rock-paper-scissors',
    name: 'Rock Paper Scissors',
    isAvailable: false
  },
  {
    id: 'memory-match',
    name: 'Memory Match',
    isAvailable: false
  }
];
