using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Interfaces;
using OnlineChessAPI.Core.Models;

namespace onlineChessAPI.Controllers;

[ApiController]
[Route("api/games/{gameId}/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _commentRepository;
    private readonly IChessGameRepository _gameRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl;
    
    public CommentsController(
        ICommentRepository commentRepository,
        IChessGameRepository gameRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _commentRepository = commentRepository;
        _gameRepository = gameRepository;
        _httpContextAccessor = httpContextAccessor;
        
        var request = _httpContextAccessor.HttpContext?.Request;
        _baseUrl = $"{request?.Scheme}://{request?.Host}{request?.PathBase}";
    }
    
    [HttpGet]
    public async Task<IActionResult> GetComments(int gameId, [FromQuery] PaginationDto paginationDto)
    {
        if (!await _gameRepository.GameExistsAsync(gameId))
        {
            return NotFound("Game not found");
        }
        
        var pagedComments = await _commentRepository.GetCommentsByGameIdAsync(gameId, paginationDto);
        
        var prevPageNumber = pagedComments.HasPrevious ? pagedComments.CurrentPage - 1 : pagedComments.CurrentPage;
        var nextPageNumber = pagedComments.HasNext ? pagedComments.CurrentPage + 1 : pagedComments.CurrentPage;
        
        pagedComments.Links.Add("self", $"{_baseUrl}/api/games/{gameId}/comments?pageNumber={pagedComments.CurrentPage}&pageSize={pagedComments.PageSize}");
        
        if (pagedComments.HasPrevious)
        {
            pagedComments.Links.Add("previous", $"{_baseUrl}/api/games/{gameId}/comments?pageNumber={prevPageNumber}&pageSize={pagedComments.PageSize}");
        }
        
        if (pagedComments.HasNext)
        {
            pagedComments.Links.Add("next", $"{_baseUrl}/api/games/{gameId}/comments?pageNumber={nextPageNumber}&pageSize={pagedComments.PageSize}");
        }
        
        var comments = pagedComments.Items.Select(comment => new CommentDto
        {
            Id = comment.Id,
            GameId = comment.GameId,
            UserId = comment.UserId,
            Username = comment.User?.Username,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        });
        
        var result = new PagedListDto<CommentDto>(
            comments, 
            pagedComments.TotalCount, 
            pagedComments.PageSize, 
            pagedComments.CurrentPage)
        {
            Links = pagedComments.Links
        };
        
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetComment(int gameId, int id)
    {
        if (!await _gameRepository.GameExistsAsync(gameId))
        {
            return NotFound("Game not found");
        }
        
        var comment = await _commentRepository.GetCommentByIdAsync(id);
        
        if (comment == null || comment.GameId != gameId)
        {
            return NotFound("Comment not found");
        }
        
        var commentDto = new CommentDto
        {
            Id = comment.Id,
            GameId = comment.GameId,
            UserId = comment.UserId,
            Username = comment.User?.Username,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
        
        return Ok(commentDto);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateComment(int gameId, [FromBody] CommentDto commentDto)
    {
        if (!await _gameRepository.GameExistsAsync(gameId))
        {
            return NotFound("Game not found");
        }
        
        int? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }
        
        var comment = new Comment
        {
            GameId = gameId,
            UserId = userId,
            Content = commentDto.Content,
            CreatedAt = DateTime.UtcNow
        };
        
        await _commentRepository.CreateCommentAsync(comment);
        
        commentDto.Id = comment.Id;
        commentDto.GameId = comment.GameId;
        commentDto.UserId = comment.UserId;
        commentDto.CreatedAt = comment.CreatedAt;
        
        return CreatedAtAction(nameof(GetComment), new { gameId, id = comment.Id }, commentDto);
    }
    
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(int gameId, int id, [FromBody] CommentDto commentDto)
    {
        if (!await _gameRepository.GameExistsAsync(gameId))
        {
            return NotFound("Game not found");
        }
        
        var comment = await _commentRepository.GetCommentByIdAsync(id);
        
        if (comment == null || comment.GameId != gameId)
        {
            return NotFound("Comment not found");
        }
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        if (comment.UserId != userId)
        {
            return Forbid();
        }
        
        comment.Content = commentDto.Content;
        
        var result = await _commentRepository.UpdateCommentAsync(comment);
        
        if (result)
        {
            return NoContent();
        }
        
        return BadRequest("Failed to update comment");
    }
    
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int gameId, int id)
    {
        if (!await _gameRepository.GameExistsAsync(gameId))
        {
            return NotFound("Game not found");
        }
        
        var comment = await _commentRepository.GetCommentByIdAsync(id);
        
        if (comment == null || comment.GameId != gameId)
        {
            return NotFound("Comment not found");
        }
        
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        if (comment.UserId != userId)
        {
            return Forbid();
        }
        
        var result = await _commentRepository.DeleteCommentAsync(id);
        
        if (result)
        {
            return NoContent();
        }
        
        return BadRequest("Failed to delete comment");
    }
}
