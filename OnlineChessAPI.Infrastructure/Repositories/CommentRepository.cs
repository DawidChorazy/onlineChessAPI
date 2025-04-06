using Microsoft.EntityFrameworkCore;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Interfaces;
using OnlineChessAPI.Core.Models;
using OnlineChessAPI.Infrastructure.Data;

namespace OnlineChessAPI.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;
    
    public CommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<PagedListDto<Comment>> GetCommentsByGameIdAsync(int gameId, PaginationDto paginationDto)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Where(c => c.GameId == gameId)
            .OrderByDescending(c => c.CreatedAt)
            .AsQueryable();
            
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var items = await query
            .Skip((paginationDto.PageNumber - 1) * paginationDto.PageSize)
            .Take(paginationDto.PageSize)
            .ToListAsync();
            
        return new PagedListDto<Comment>(items, totalCount, paginationDto.PageSize, paginationDto.PageNumber);
    }
    
    public async Task<Comment?> GetCommentByIdAsync(int id)
    {
        return await _context.Comments
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Comment> CreateCommentAsync(Comment comment)
    {
        comment.CreatedAt = DateTime.UtcNow;
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
        return comment;
    }
    
    public async Task<bool> UpdateCommentAsync(Comment comment)
    {
        comment.UpdatedAt = DateTime.UtcNow;
        _context.Comments.Update(comment);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteCommentAsync(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return false;
            
        _context.Comments.Remove(comment);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> CommentExistsAsync(int id)
    {
        return await _context.Comments.AnyAsync(c => c.Id == id);
    }
    
    public async Task<bool> UserOwnsCommentAsync(int userId, int commentId)
    {
        return await _context.Comments.AnyAsync(c => c.Id == commentId && c.UserId == userId);
    }
}
