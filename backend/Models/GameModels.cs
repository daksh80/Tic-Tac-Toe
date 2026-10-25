namespace backend.Models;

public sealed record GameMove(int MoveNumber, string Player, int Row, int Column);

public sealed record GameSnapshot(
    Guid SessionId,
    string Mode,
    int Rows,
    int Columns,
    IReadOnlyList<string?> Board,
    string CurrentPlayer,
    string Status,
    string? Winner,
    IReadOnlyList<int> WinningCells,
    IReadOnlyList<GameMove> Moves);

public sealed record Scoreboard(int XWins, int OWins, int Draws);

public sealed record ResetGameRequest(string Mode, int Rows = 3, int Columns = 3);

public sealed record PlayMoveRequest(int CellIndex);
