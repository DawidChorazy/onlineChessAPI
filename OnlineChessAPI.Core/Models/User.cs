using System.ComponentModel.DataAnnotations;

namespace OnlineChessAPI.Core.Models;

public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
    
    [Required]
    public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<ChessGame> WhiteGames { get; set; } = new List<ChessGame>();
    public ICollection<ChessGame> BlackGames { get; set; } = new List<ChessGame>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
