using backend.Services;
using Xunit;

namespace backend.tests;

public sealed class GameSessionServiceTests
{
    [Fact]
    public void NewGameStartsWithEmptyBoardAndXTurn()
    {
        var service = new GameSessionService();
        var game = service.GetGame();

        Assert.Equal("X", game.CurrentPlayer);
        Assert.Equal("inProgress", game.Status);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Empty(game.Moves);
    }

    [Fact]
    public void ValidMoveRecordsPositionAndSwitchesTurn()
    {
        var service = new GameSessionService();

        var game = service.Play(0);

        Assert.Equal("X", game.Board[0]);
        Assert.Equal("O", game.CurrentPlayer);
        Assert.Single(game.Moves);
        Assert.Equal(("X", 1, 1), (game.Moves[0].Player, game.Moves[0].Row, game.Moves[0].Column));
    }

    [Fact]
    public void InvalidMovesDoNotChangeGame()
    {
        var service = new GameSessionService();
        service.Play(0);
        var gameBeforeInvalidMove = service.GetGame();

        Assert.Throws<InvalidOperationException>(() => service.Play(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => service.Play(9));

        var gameAfterInvalidMove = service.GetGame();
        Assert.Equal(gameBeforeInvalidMove.CurrentPlayer, gameAfterInvalidMove.CurrentPlayer);
        Assert.Equal(gameBeforeInvalidMove.Board, gameAfterInvalidMove.Board);
        Assert.Equal(gameBeforeInvalidMove.Moves, gameAfterInvalidMove.Moves);
    }

    [Fact]
    public void DetectsRowWinAndUpdatesScoreOnce()
    {
        var service = new GameSessionService();
        PlayAll(service, 0, 3, 1, 4, 2);

        var game = service.GetGame();
        Assert.Equal("won", game.Status);
        Assert.Equal("X", game.Winner);
        Assert.Equal(new[] { 0, 1, 2 }, game.WinningCells);
        Assert.Equal(1, service.GetScoreboard().XWins);
        Assert.Throws<InvalidOperationException>(() => service.Play(5));
        Assert.Equal(1, service.GetScoreboard().XWins);
    }

    [Fact]
    public void DetectsColumnWin()
    {
        var service = new GameSessionService();
        PlayAll(service, 0, 1, 3, 2, 6);

        Assert.Equal("X", service.GetGame().Winner);
        Assert.Equal(new[] { 0, 3, 6 }, service.GetGame().WinningCells);
    }

    [Fact]
    public void DetectsDiagonalWin()
    {
        var service = new GameSessionService();
        PlayAll(service, 0, 1, 4, 2, 8);

        Assert.Equal("X", service.GetGame().Winner);
        Assert.Equal(new[] { 0, 4, 8 }, service.GetGame().WinningCells);
    }

    [Fact]
    public void DetectsDrawAndUpdatesScore()
    {
        var service = new GameSessionService();
        PlayAll(service, 0, 1, 2, 4, 3, 5, 7, 6, 8);

        Assert.Equal("draw", service.GetGame().Status);
        Assert.Equal(1, service.GetScoreboard().Draws);
        Assert.Throws<InvalidOperationException>(() => service.Play(0));
    }

    [Fact]
    public void ResetStartsNewGameAndKeepsScoreboard()
    {
        var service = new GameSessionService();
        PlayAll(service, 0, 3, 1, 4, 2);

        var game = service.Reset("twoPlayer");

        Assert.Equal("X", game.CurrentPlayer);
        Assert.Equal("inProgress", game.Status);
        Assert.Empty(game.Moves);
        Assert.All(game.Board, cell => Assert.Null(cell));
        Assert.Equal(1, service.GetScoreboard().XWins);
    }

    [Fact]
    public void RectangularGridUsesRowsColumnsAndDetectsFullRowWin()
    {
        var service = new GameSessionService();
        var game = service.Reset("twoPlayer", 3, 4);

        Assert.Equal(3, game.Rows);
        Assert.Equal(4, game.Columns);
        Assert.Equal(12, game.Board.Count);

        PlayAll(service, 0, 4, 1, 5, 2, 6, 3);

        game = service.GetGame();
        Assert.Equal("won", game.Status);
        Assert.Equal("X", game.Winner);
        Assert.Equal(new[] { 0, 1, 2, 3 }, game.WinningCells);
        Assert.Equal((1, 4), (game.Moves[^1].Row, game.Moves[^1].Column));
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(3, 11)]
    public void ResetRejectsDimensionsOutsideSupportedRange(int rows, int columns)
    {
        var service = new GameSessionService();

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Reset("twoPlayer", rows, columns));
    }

    [Fact]
    public void RectangularGridDoesNotTreatPartialDiagonalAsWin()
    {
        var service = new GameSessionService();
        service.Reset("twoPlayer", 3, 4);

        PlayAll(service, 0, 1, 5, 2, 10, 3);

        Assert.Equal("inProgress", service.GetGame().Status);
    }

    [Fact]
    public void UndoUsesRectangularColumnCount()
    {
        var service = new GameSessionService();
        service.Reset("twoPlayer", 3, 4);
        service.Play(7);

        var game = service.Undo();

        Assert.Null(game.Board[7]);
        Assert.Empty(game.Moves);
        Assert.Equal("X", game.CurrentPlayer);
    }

    [Fact]
    public void UndoInTwoPlayerModeRemovesOnlyLastMove()
    {
        var service = new GameSessionService();
        service.Play(0);
        service.Play(4);

        var game = service.Undo();

        Assert.Equal("O", game.CurrentPlayer);
        Assert.Null(game.Board[4]);
        Assert.Equal("X", game.Board[0]);
        Assert.Single(game.Moves);
    }

    [Fact]
    public void UndoInComputerModeRemovesHumanAndComputerPair()
    {
        var service = new GameSessionService();
        service.Reset("computer");
        service.Play(0);

        var game = service.Undo();

        Assert.Equal("X", game.CurrentPlayer);
        Assert.Empty(game.Moves);
        Assert.All(game.Board, cell => Assert.Null(cell));
    }

    [Fact]
    public void UndoAfterWinReversesScore()
    {
        var service = new GameSessionService();
        PlayAll(service, 0, 3, 1, 4, 2);

        var game = service.Undo();

        Assert.Equal("inProgress", game.Status);
        Assert.Equal(0, service.GetScoreboard().XWins);
        Assert.Equal("X", game.CurrentPlayer);
    }

    [Fact]
    public void UndoIsRejectedWhenThereAreNoMoves()
    {
        var service = new GameSessionService();
        Assert.Throws<InvalidOperationException>(() => service.Undo());
    }

    [Fact]
    public void ComputerTakesCenterWhenAvailable()
    {
        var service = new GameSessionService();
        service.Reset("computer");

        var game = service.Play(0);

        Assert.Equal("O", game.Board[4]);
        Assert.Equal(2, game.Moves.Count);
    }

    [Fact]
    public void ComputerTakesWinningMoveBeforeBlocking()
    {
        var service = new GameSessionService();
        service.Reset("computer");
        service.Play(0);
        service.Play(8);

        var game = service.Play(1);

        Assert.Equal("O", game.Board[6]);
        Assert.Equal("won", game.Status);
        Assert.Equal("O", game.Winner);
    }

    [Fact]
    public void ComputerBlocksHumanWinningMove()
    {
        var service = new GameSessionService();
        service.Reset("computer");
        service.Play(0);
        service.Play(8);

        var game = service.Play(6);

        Assert.Equal("O", game.Board[7]);
        Assert.Equal("inProgress", game.Status);
    }

    [Fact]
    public void ResetScoreboardClearsScores()
    {
        var service = new GameSessionService();
        PlayAll(service, 0, 3, 1, 4, 2);

        var scoreboard = service.ResetScoreboard();

        Assert.Equal((0, 0, 0), (scoreboard.XWins, scoreboard.OWins, scoreboard.Draws));
    }

    private static void PlayAll(GameSessionService service, params int[] cells)
    {
        foreach (var cell in cells)
        {
            service.Play(cell);
        }
    }
}
