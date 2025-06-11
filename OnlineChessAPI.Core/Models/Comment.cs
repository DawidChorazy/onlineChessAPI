using System.ComponentModel.DataAnnotations;

namespace OnlineChessAPI.Core.Models;

public class Comment
{
    [Key]
    public int Id { get; set; }
    
    public int GameId { get; set; }
    
    public int? UserId { get; set; }
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    public ChessGame? Game { get; set; }
    public User? User { get; set; }
}
