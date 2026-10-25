using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api")]
public sealed class GameController(GameSessionService gameSession) : ControllerBase
{
    [HttpGet("game")]
    public ActionResult<GameSnapshot> GetGame() => Ok(gameSession.GetGame());

    [HttpPost("game/reset")]
    public ActionResult<GameSnapshot> ResetGame([FromBody] ResetGameRequest request)
    {
        try
        {
            return Ok(gameSession.Reset(request.Mode, request.Rows, request.Columns));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid game settings", Detail = exception.Message });
        }
    }

    [HttpPost("game/moves")]
    public ActionResult<GameSnapshot> PlayMove([FromBody] PlayMoveRequest request)
    {
        try
        {
            return Ok(gameSession.Play(request.CellIndex));
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid cell", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Move rejected", Detail = exception.Message });
        }
    }

    [HttpPost("game/undo")]
    public ActionResult<GameSnapshot> UndoMove()
    {
        try
        {
            return Ok(gameSession.Undo());
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Undo rejected", Detail = exception.Message });
        }
    }

    [HttpGet("scoreboard")]
    public ActionResult<Scoreboard> GetScoreboard() => Ok(gameSession.GetScoreboard());

    [HttpPost("scoreboard/reset")]
    public ActionResult<Scoreboard> ResetScoreboard() => Ok(gameSession.ResetScoreboard());
}
