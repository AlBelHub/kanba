using System.Data;
using backend.Helpers;
using backend.Models;
using backend.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : KanbaControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthController(IDbConnection db, IUserRepository userRepository, ITokenGenerator tokenGenerator) : base(db)
    {
        _userRepository = userRepository;
        _tokenGenerator = tokenGenerator;
    }


    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] UserModel login)
    {
        if (string.IsNullOrWhiteSpace(login.username) || string.IsNullOrWhiteSpace(login.password))
        {
            return BadRequest("Invalid credentials");
        }

        if (!await _userRepository.UserExistsAsync(login.username))
        {
            return Unauthorized("Пользователь не зарегистрирован");
        }

        if (await _userRepository.VerifyPasswordHashAsync(login.username, login.password))
        {
            return Unauthorized("Введен неверный пароль");
        }

        var token = _tokenGenerator.GenerateToken(login);
        return Ok(new { token });
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] UserModel newUser)
    {
        if (string.IsNullOrWhiteSpace(newUser.username) || string.IsNullOrWhiteSpace(newUser.password))
        {
            return BadRequest("Необходимо ввести имя и пароль");
        }

        if (await _userRepository.UserExistsAsync(newUser.username))
        {
            return BadRequest("Такой пользователь уже существует");
        }

        var user = await _userRepository.RegisterUserAsync(newUser.username, newUser.password);
        if (user == null)
        {
            return BadRequest("Что-то пошло не так");
        }

        return Ok("Пользователь зарегистрирован!.");
    }

    [HttpGet("validate_token")]
    [Authorize]
    public async Task<IActionResult> ValidateToken()
    {
        return Ok();
    }
    
    
}