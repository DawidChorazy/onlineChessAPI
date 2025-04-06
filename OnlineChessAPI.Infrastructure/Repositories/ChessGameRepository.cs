using Microsoft.EntityFrameworkCore;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Interfaces;
using OnlineChessAPI.Core.Models;
using OnlineChessAPI.Infrastructure.Data;

namespace OnlineChessAPI.Infrastructure.Repositories;

public class ChessGameRepository : IChessGameRepository
{
    private readonly ApplicationDbContext _context;
    
    public ChessGameRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<PagedListDto<ChessGame>> GetGamesAsync(PaginationDto paginationDto, string? sortBy = null, string? filterBy = null)
    {
        var query = _context.ChessGames
            .Include(g => g.WhiteUser)
            .Include(g => g.BlackUser)
            .AsQueryable();
            
        // Apply filtering if provided
        if (!string.IsNullOrWhiteSpace(filterBy))
        {
            // Parse filterBy string (format: "property=value")
            var filterParts = filterBy.Split('=');
            if (filterParts.Length == 2)
            {
                var property = filterParts[0].Trim();
                var value = filterParts[1].Trim();
                
                query = property.ToLower() switch
                {
                    "rated" => bool.TryParse(value, out var rated) 
                        ? query.Where(g => g.Rated == rated) 
                        : query,
                    "victorystatus" => query.Where(g => g.VictoryStatus != null && g.VictoryStatus.Contains(value)),
                    "winner" => query.Where(g => g.Winner != null && g.Winner.Contains(value)),
                    "openingcode" => query.Where(g => g.OpeningCode != null && g.OpeningCode.Contains(value)),
                    "openingfullname" => query.Where(g => g.OpeningFullname != null && g.OpeningFullname.Contains(value)),
                    _ => query
                };
            }
        }
        
        // Apply sorting if provided
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            var isDescending = sortBy.StartsWith("-");
            var propertyName = isDescending ? sortBy[1..] : sortBy;
            
            query = propertyName.ToLower() switch
            {
                "gameid" => isDescending 
                    ? query.OrderByDescending(g => g.GameId) 
                    : query.OrderBy(g => g.GameId),
                "turns" => isDescending
                    ? query.OrderByDescending(g => g.Turns)
                    : query.OrderBy(g => g.Turns),
                "whiterating" => isDescending
                    ? query.OrderByDescending(g => g.WhiteRating)
                    : query.OrderBy(g => g.WhiteRating),
                "blackrating" => isDescending
                    ? query.OrderByDescending(g => g.BlackRating)
                    : query.OrderBy(g => g.BlackRating),
                _ => query.OrderBy(g => g.GameId)
            };
        }
        else
        {
            // Default sorting by GameId
            query = query.OrderBy(g => g.GameId);
        }
        
        // Get total count before pagination
        var totalCount = await query.CountAsync();
        
        // Apply pagination
        var items = await query
            .Skip((paginationDto.PageNumber - 1) * paginationDto.PageSize)
            .Take(paginationDto.PageSize)
            .ToListAsync();
            
        return new PagedListDto<ChessGame>(items, totalCount, paginationDto.PageSize, paginationDto.PageNumber);
    }
    
    public async Task<ChessGame?> GetGameByIdAsync(int id)
    {
        return await _context.ChessGames
            .Include(g => g.WhiteUser)
            .Include(g => g.BlackUser)
            .FirstOrDefaultAsync(g => g.GameId == id);
    }
    
    public async Task<ChessGame> CreateGameAsync(ChessGame game)
    {
        await _context.ChessGames.AddAsync(game);
        await _context.SaveChangesAsync();
        return game;
    }
    
    public async Task<bool> UpdateGameAsync(ChessGame game)
    {
        _context.ChessGames.Update(game);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteGameAsync(int id)
    {
        var game = await _context.ChessGames.FindAsync(id);
        if (game == null)
            return false;
            
        _context.ChessGames.Remove(game);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> GameExistsAsync(int id)
    {
        return await _context.ChessGames.AnyAsync(g => g.GameId == id);
    }
}
