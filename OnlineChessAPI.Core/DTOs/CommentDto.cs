using System.ComponentModel.DataAnnotations;

namespace OnlineChessAPI.Core.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int? UserId { get; set; }
    public string? Username { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
