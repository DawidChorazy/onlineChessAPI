using System.ComponentModel.DataAnnotations;

namespace OnlineChessAPI.Core.DTOs;

public class UserLoginDto
{
    [Required]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}
