# Tic-Tac-Toe

A small full-stack Tic-Tac-Toe game. The Angular frontend supports two-player and computer modes, variable board dimensions, move history, undo, and a session scoreboard. The .NET API owns the game rules and state.

## Repository layout

```text
frontend/       Angular application
backend/        ASP.NET Core Web API
backend.tests/  xUnit tests for game logic
vercel.json     Vercel frontend and API service routing
```

## Requirements

- Node.js 20 or newer and npm
- .NET SDK 10.0

## Run locally

Open two terminals from the repository root.

Start the API:

```bash
dotnet run --project backend/backend.csproj --urls http://localhost:5147
```

Install and start the Angular app:

```bash
cd frontend
npm ci
npm start
```

Open <http://localhost:4200>. Angular's development proxy forwards `/api` requests to `http://localhost:5147` (configured in `frontend/proxy.conf.json`).

## Build and tests

Build the frontend:

```bash
cd frontend
npm ci
npm run build
```

Run the backend unit tests:

```bash
dotnet test backend.tests/backend.tests.csproj
```

Run the Angular unit tests in a browser with Karma:

```bash
cd frontend
npm test -- --watch=false --browsers=ChromeHeadless
```

Chrome or Chromium must be installed for the headless browser command. The backend tests cover game rules, wins and draws, computer moves, rectangular boards, reset, and undo. Angular's current test suite contains basic component creation tests.

## API summary

All routes are prefixed with `/api`. JSON uses camelCase. Game state is held by the backend process.

| Method | Route | Description |
| --- | --- | --- |
| `GET` | `/api/game` | Get the current game snapshot. |
| `POST` | `/api/game/reset` | Start a new game with a mode and board dimensions. |
| `POST` | `/api/game/moves` | Play a move at a zero-based board cell index. |
| `POST` | `/api/game/undo` | Undo the last move (or the human/computer turn pair). |
| `GET` | `/api/scoreboard` | Get the current process's scores. |
| `POST` | `/api/scoreboard/reset` | Clear the scores. |

### Request examples

Start a 4 by 4 two-player game:

```json
{
  "mode": "twoPlayer",
  "rows": 4,
  "columns": 4
}
```

`mode` must be `twoPlayer` or `computer`; rows and columns must each be from 3 to 10.

Play the first cell (indices start at zero):

```json
{
  "cellIndex": 0
}
```

### Game response

Game routes return a snapshot containing `sessionId`, `mode`, `rows`, `columns`, `board`, `currentPlayer`, `status`, `winner`, `winningCells`, and `moves`. Board entries are `"X"`, `"O"`, or `null`; winning cell and move row/column positions are one-based only for the move coordinates. `status` is `inProgress`, `won`, or `draw`.

The scoreboard response has this shape:

```json
{
  "xWins": 0,
  "oWins": 0,
  "draws": 0
}
```

Invalid game settings or cell indexes return `400 Bad Request`. A move that cannot be played, or an undo when there are no moves, returns `409 Conflict`. API errors use ASP.NET `ProblemDetails` responses.

## AI workflow notes

AI assistance was used as a coding aid during implementation and documentation. The workflow focused on small, reviewable tasks: fix dynamic board sizing and rendering, simplify the Angular component and move reusable API/session logic into services, centralize shared constants and types, and document the backend API and run steps. Changes were checked against the source, and backend game behavior has xUnit coverage. AI output should be reviewed like any other code contribution.

## Assumptions and limitations

- The scoreboard and active game are stored in memory in a singleton service. They reset when the backend restarts, are not shared across multiple backend instances, and are not persistent between users.
- There is one active game per backend process; this is not a multiplayer service with separate persisted sessions.
- The computer player uses a basic game strategy and is not guaranteed to play optimally on larger boards.
- Board dimensions are rectangular in the API, while the frontend's grid-size control creates square boards.
- The game list includes placeholder entries for games that are not implemented.
- CORS currently allows the local Angular origin and the configured Vercel frontend origins. Update the allow-list if the frontend is moved to a different origin and calls the backend directly.
- Vercel deployment uses the root `vercel.json` to route `/api/*` to the .NET container service and other paths to the Angular frontend. The in-memory state limitations still apply there.
