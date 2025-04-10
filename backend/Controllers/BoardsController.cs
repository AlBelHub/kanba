using System.Data;
using backend.Models;
using backend.Repositories.Interfaces;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BoardsController : KanbaControllerBase
{
    private readonly IBoardRepository _boardRepository;
    
    public BoardsController(IDbConnection db, IBoardRepository boardRepository) : base(db)
    {
        _boardRepository = boardRepository;
    }
    
    [HttpGet("getBoards/{space_id:guid}")]
    public async Task<IActionResult> GetBoards(Guid space_id)
    {
        var boards = await _db.QueryAsync("SELECT id, name, space_id, owner_id FROM Boards WHERE space_id = @space_id", new { space_id });
        return Ok(boards);
    }

    [HttpPost("createBoard")]
    public async Task<IActionResult> CreateBoard([FromBody] BoardsProps props)
    {
        
        var newBoard = await _boardRepository.CreateBoard(props);

        Console.WriteLine(newBoard);
        
        if (newBoard != null)
        {
            return Ok(newBoard);
        }
        else
        {
            return BadRequest("Что-то пошло не так при создании доски");
        }
    }
}