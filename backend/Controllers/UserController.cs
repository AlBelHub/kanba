using System.Data;
using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : KanbaControllerBase
{
    private readonly IUserRepository _userRepository;
    
    public UserController(IDbConnection db, IUserRepository userRepository) : base(db)
    {
        _userRepository = userRepository;
    }

    [HttpGet("getUserId/{username}")]
    public async Task<IActionResult> GetUserId(string username)
    {
        var userId = await _userRepository.GetUserId(username);
        return Ok(userId);
    }

    [HttpGet("getUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetUsersAsync();
        return Ok(users);
    }
}