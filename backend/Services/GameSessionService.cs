using backend.Models;

namespace backend.Services;

public sealed class GameSessionService
{
    private readonly object _sync = new();
    private string?[] _board = new string?[3 * 3];
    private readonly List<GameMove> _moves = [];
    private Guid _sessionId = Guid.NewGuid();
    private string _mode = "twoPlayer";
    private int _rows = 3;
    private int _columns = 3;
    private string _currentPlayer = "X";
    private string _status = "inProgress";
    private string? _winner;
    private int[] _winningCells = [];
    private string? _recordedResult;
    private int _xWins;
    private int _oWins;
    private int _draws;

    public GameSnapshot GetGame()
    {
        lock (_sync)
        {
            return CreateSnapshot();
        }
    }

    public Scoreboard GetScoreboard()
    {
        lock (_sync)
        {
            return CreateScoreboard();
        }
    }

    public Scoreboard ResetScoreboard()
    {
        lock (_sync)
        {
            _xWins = 0;
            _oWins = 0;
            _draws = 0;
            _recordedResult = null;
            return CreateScoreboard();
        }
    }

    public GameSnapshot Reset(string mode, int rows = 3, int columns = 3)
    {
        if (mode is not ("twoPlayer" or "computer"))
        {
            throw new ArgumentException("Mode must be 'twoPlayer' or 'computer'.", nameof(mode));
        }
        if (rows is < 3 or > 10 || columns is < 3 or > 10)
        {
            throw new ArgumentOutOfRangeException(nameof(rows), "Rows and columns must each be between 3 and 10.");
        }

        lock (_sync)
        {
            _board = new string?[rows * columns];
            _moves.Clear();
            _sessionId = Guid.NewGuid();
            _mode = mode;
            _rows = rows;
            _columns = columns;
            _currentPlayer = "X";
            _status = "inProgress";
            _winner = null;
            _winningCells = [];
            _recordedResult = null;
            return CreateSnapshot();
        }
    }

    public GameSnapshot Play(int cellIndex)
    {
        lock (_sync)
        {
            if (cellIndex < 0 || cellIndex >= _board.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(cellIndex), $"Cell index must be between 0 and {_board.Length - 1}.");
            }

            if (_status != "inProgress")
            {
                throw new InvalidOperationException("The game is already complete. Reset to start a new game.");
            }

            if (_mode == "computer" && _currentPlayer == "O")
            {
                throw new InvalidOperationException("It is the computer's turn.");
            }

            if (_board[cellIndex] is not null)
            {
                throw new InvalidOperationException("That cell is already occupied.");
            }

            MakeMove(cellIndex, _currentPlayer);
            UpdateGameStatus();
            UpdateRecordedScore();

            if (_mode == "computer" && _status == "inProgress")
            {
                var computerMove = FindComputerMove();
                MakeMove(computerMove, "O");
                UpdateGameStatus();
                UpdateRecordedScore();
            }

            return CreateSnapshot();
        }
    }

    public GameSnapshot Undo()
    {
        lock (_sync)
        {
            if (_moves.Count == 0)
            {
                throw new InvalidOperationException("There are no moves to undo.");
            }

            var movesToRemove = 1;
            if (_mode == "computer" && _moves[^1].Player == "O")
            {
                movesToRemove = 2;
            }

            var previousResult = _recordedResult;
            for (var i = 0; i < movesToRemove && _moves.Count > 0; i++)
            {
                var move = _moves[^1];
                _board[(move.Row - 1) * _columns + move.Column - 1] = null;
                _moves.RemoveAt(_moves.Count - 1);
            }

            _status = "inProgress";
            _winner = null;
            _winningCells = [];
            _currentPlayer = _moves.Count == 0 || _moves[^1].Player == "O" ? "X" : "O";
            UpdateGameStatus();
            _recordedResult = previousResult;
            UpdateRecordedScore();
            return CreateSnapshot();
        }
    }

    private void MakeMove(int cellIndex, string player)
    {
        _board[cellIndex] = player;
        _moves.Add(new GameMove(
            _moves.Count + 1,
            player,
            cellIndex / _columns + 1,
            cellIndex % _columns + 1));
        _currentPlayer = player == "X" ? "O" : "X";
    }

    private void UpdateGameStatus()
    {
        foreach (var line in GetWinningLines())
        {
            var mark = _board[line[0]];
            if (mark is not null && line.All(index => _board[index] == mark))
            {
                _status = "won";
                _winner = mark;
                _winningCells = line;
                return;
            }
        }

        if (_board.All(cell => cell is not null))
        {
            _status = "draw";
            _winner = null;
            _winningCells = [];
            return;
        }

        _status = "inProgress";
        _winner = null;
        _winningCells = [];
    }

    private void UpdateRecordedScore()
    {
        var result = _status switch
        {
            "won" => _winner,
            "draw" => "draw",
            _ => null
        };

        if (_recordedResult == result)
        {
            return;
        }

        AdjustScore(_recordedResult, -1);
        AdjustScore(result, 1);
        _recordedResult = result;
    }

    private void AdjustScore(string? result, int delta)
    {
        switch (result)
        {
            case "X":
                _xWins += delta;
                break;
            case "O":
                _oWins += delta;
                break;
            case "draw":
                _draws += delta;
                break;
        }
    }

    private IEnumerable<int[]> GetWinningLines()
    {
        for (var row = 0; row < _rows; row++)
        {
            yield return Enumerable.Range(row * _columns, _columns).ToArray();
        }

        for (var column = 0; column < _columns; column++)
        {
            yield return Enumerable.Range(0, _rows)
                .Select(row => row * _columns + column)
                .ToArray();
        }

        if (_rows == _columns)
        {
            yield return Enumerable.Range(0, _rows)
                .Select(index => index * _columns + index)
                .ToArray();
            yield return Enumerable.Range(0, _rows)
                .Select(index => index * _columns + _columns - index - 1)
                .ToArray();
        }
    }

    private int FindComputerMove()
    {
        var winningMove = FindTacticalMove("O");
        if (winningMove is not null)
        {
            return winningMove.Value;
        }

        var blockingMove = FindTacticalMove("X");
        if (blockingMove is not null)
        {
            return blockingMove.Value;
        }

        var center = (_rows / 2) * _columns + _columns / 2;
        if (_board[center] is null)
        {
            return center;
        }

        foreach (var corner in new[] { 0, _columns - 1, _board.Length - _columns, _board.Length - 1 }.Distinct())
        {
            if (_board[corner] is null)
            {
                return corner;
            }
        }

        return Array.FindIndex(_board, cell => cell is null);
    }

    private int? FindTacticalMove(string player)
    {
        foreach (var line in GetWinningLines())
        {
            var playerCells = line.Where(index => _board[index] == player).ToArray();
            var emptyCells = line.Where(index => _board[index] is null).ToArray();
            if (playerCells.Length == line.Length - 1 && emptyCells.Length == 1)
            {
                return emptyCells[0];
            }
        }

        return null;
    }

    private GameSnapshot CreateSnapshot() => new(
        _sessionId,
        _mode,
        _rows,
        _columns,
        _board.ToArray(),
        _currentPlayer,
        _status,
        _winner,
        _winningCells.ToArray(),
        _moves.ToArray());

    private Scoreboard CreateScoreboard() => new(_xWins, _oWins, _draws);
}
