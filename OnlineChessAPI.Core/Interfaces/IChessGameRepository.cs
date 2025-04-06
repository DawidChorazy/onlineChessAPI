using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Models;

namespace OnlineChessAPI.Core.Interfaces;

public interface IChessGameRepository
{
    Task<PagedListDto<ChessGame>> GetGamesAsync(PaginationDto paginationDto, string? sortBy = null, string? filterBy = null);
    Task<ChessGame?> GetGameByIdAsync(int id);
    Task<ChessGame> CreateGameAsync(ChessGame game);
    Task<bool> UpdateGameAsync(ChessGame game);
    Task<bool> DeleteGameAsync(int id);
    Task<bool> GameExistsAsync(int id);
}
