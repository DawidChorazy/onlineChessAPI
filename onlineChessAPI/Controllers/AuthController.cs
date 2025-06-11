using Microsoft.AspNetCore.Mvc;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Infrastructure.Services;

namespace onlineChessAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register(UserRegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        
        if (!result.Success)
        {
            return BadRequest(new { message = result.Message });
        }
        
        return Ok(new { token = result.Token, message = result.Message });
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        
        if (!result.Success)
        {
            return Unauthorized(new { message = result.Message });
        }
        
        return Ok(new
        {
            token = result.Token,
            user = new
            {
                id = result.User!.Id,
                username = result.User.Username,
                email = result.User.Email
            }
        });
    }
}
