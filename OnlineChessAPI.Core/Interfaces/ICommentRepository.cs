using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Models;

namespace OnlineChessAPI.Core.Interfaces;

public interface ICommentRepository
{
    Task<PagedListDto<Comment>> GetCommentsByGameIdAsync(int gameId, PaginationDto paginationDto);
    Task<Comment?> GetCommentByIdAsync(int id);
    Task<Comment> CreateCommentAsync(Comment comment);
    Task<bool> UpdateCommentAsync(Comment comment);
    Task<bool> DeleteCommentAsync(int id);
    Task<bool> CommentExistsAsync(int id);
    Task<bool> UserOwnsCommentAsync(int userId, int commentId);
}
