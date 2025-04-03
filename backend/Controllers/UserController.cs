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

    [HttpGet("getUsers")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userRepository.GetUsersAsync();
        return Ok(users);
    }
}