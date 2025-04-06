using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Models;
using OnlineChessAPI.Infrastructure.Data;

namespace OnlineChessAPI.Infrastructure.Services;

public class AuthService
{
    private readonly ApplicationDbContext _context;
    private readonly TokenService _tokenService;
    
    public AuthService(ApplicationDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }
    
    public async Task<(bool Success, string Message, string? Token)> RegisterAsync(UserRegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Username.ToLower() == registerDto.Username.ToLower()))
        {
            return (false, "Username is already taken", null);
        }
        
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == registerDto.Email.ToLower()))
        {
            return (false, "Email is already registered", null);
        }
        
        using var hmac = new HMACSHA512();
        
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        
        return (true, "User registered successfully", _tokenService.CreateToken(user));
    }
    
    public async Task<(bool Success, string Message, string? Token, User? User)> LoginAsync(UserLoginDto loginDto)
    {
        var user = await _context.Users
            .SingleOrDefaultAsync(u => u.Username.ToLower() == loginDto.Username.ToLower());
            
        if (user == null)
        {
            return (false, "Invalid username or password", null, null);
        }
        
        using var hmac = new HMACSHA512(user.PasswordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));
        
        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i])
            {
                return (false, "Invalid username or password", null, null);
            }
        }
        
        return (true, "Login successful", _tokenService.CreateToken(user), user);
    }
}
