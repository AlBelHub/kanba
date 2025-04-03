using System.Data;
using backend.Models;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpaceController : KanbaControllerBase
{
    public SpaceController(IDbConnection db) : base(db) { }

    [HttpGet("getSpaces")]
    public async Task<IActionResult> GetSpaces()
    {
        var spaces = await _db.QueryAsync("SELECT id, name, user_id FROM Spaces");
        return Ok(spaces);
    }

    [HttpGet("getSpacesNyUserId/{userId:guid}")]
    public async Task<IActionResult> GetSpacesByUserId(Guid userId)
    {
        var spaces = await _db.QueryAsync<Space>("SELECT id, name, user_id, created_at FROM Spaces WHERE user_id = @user_id", new { userId = userId });
        if (!spaces.Any()) return NotFound();
        return Ok(spaces);
    }
}