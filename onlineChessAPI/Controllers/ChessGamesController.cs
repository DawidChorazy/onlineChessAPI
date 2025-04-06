using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Interfaces;
using OnlineChessAPI.Core.Models;

namespace onlineChessAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChessGamesController : ControllerBase
{
    private readonly IChessGameRepository _gameRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string _baseUrl;
    
    public ChessGamesController(
        IChessGameRepository gameRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _gameRepository = gameRepository;
        _httpContextAccessor = httpContextAccessor;
        
        var request = _httpContextAccessor.HttpContext?.Request;
        _baseUrl = $"{request?.Scheme}://{request?.Host}{request?.PathBase}";
    }
    
    [HttpGet]
    public async Task<IActionResult> GetGames([FromQuery] PaginationDto paginationDto, 
                                           [FromQuery] string? sortBy = null,
                                           [FromQuery] string? filterBy = null)
    {
        var pagedGames = await _gameRepository.GetGamesAsync(paginationDto, sortBy, filterBy);
        
        // Add HATEOAS links
        var prevPageNumber = pagedGames.HasPrevious ? pagedGames.CurrentPage - 1 : pagedGames.CurrentPage;
        var nextPageNumber = pagedGames.HasNext ? pagedGames.CurrentPage + 1 : pagedGames.CurrentPage;
        
        var queryParams = GetQueryParams(sortBy, filterBy);
        
        pagedGames.Links.Add("self", $"{_baseUrl}/api/ChessGames?pageNumber={pagedGames.CurrentPage}&pageSize={pagedGames.PageSize}{queryParams}");
        
        if (pagedGames.HasPrevious)
        {
            pagedGames.Links.Add("previous", $"{_baseUrl}/api/ChessGames?pageNumber={prevPageNumber}&pageSize={pagedGames.PageSize}{queryParams}");
        }
        
        if (pagedGames.HasNext)
        {
            pagedGames.Links.Add("next", $"{_baseUrl}/api/ChessGames?pageNumber={nextPageNumber}&pageSize={pagedGames.PageSize}{queryParams}");
        }
        
        // Convert to DTOs
        var games = pagedGames.Items.Select(game => new ChessGameDto
        {
            GameId = game.GameId,
            Rated = game.Rated,
            Turns = game.Turns,
            VictoryStatus = game.VictoryStatus,
            Winner = game.Winner,
            TimeIncrement = game.TimeIncrement,
            WhiteId = game.WhiteId,
            WhiteRating = game.WhiteRating,
            BlackId = game.BlackId,
            BlackRating = game.BlackRating,
            Moves = game.Moves,
            OpeningCode = game.OpeningCode,
            OpeningMoves = game.OpeningMoves,
            OpeningFullname = game.OpeningFullname,
            OpeningShortname = game.OpeningShortname,
            OpeningResponse = game.OpeningResponse,
            OpeningVariation = game.OpeningVariation
        });
        
        var result = new PagedListDto<ChessGameDto>(
            games, 
            pagedGames.TotalCount, 
            pagedGames.PageSize, 
            pagedGames.CurrentPage)
        {
            Links = pagedGames.Links
        };
        
        return Ok(result);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetGame(int id)
    {
        var game = await _gameRepository.GetGameByIdAsync(id);
        
        if (game == null)
        {
            return NotFound();
        }
        
        var gameDto = new ChessGameDto
        {
            GameId = game.GameId,
            Rated = game.Rated,
            Turns = game.Turns,
            VictoryStatus = game.VictoryStatus,
            Winner = game.Winner,
            TimeIncrement = game.TimeIncrement,
            WhiteId = game.WhiteId,
            WhiteRating = game.WhiteRating,
            BlackId = game.BlackId,
            BlackRating = game.BlackRating,
            Moves = game.Moves,
            OpeningCode = game.OpeningCode,
            OpeningMoves = game.OpeningMoves,
            OpeningFullname = game.OpeningFullname,
            OpeningShortname = game.OpeningShortname,
            OpeningResponse = game.OpeningResponse,
            OpeningVariation = game.OpeningVariation
        };
        
        return Ok(gameDto);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateGame(ChessGameDto gameDto)
    {
        var game = new ChessGame
        {
            Rated = gameDto.Rated,
            Turns = gameDto.Turns,
            VictoryStatus = gameDto.VictoryStatus,
            Winner = gameDto.Winner,
            TimeIncrement = gameDto.TimeIncrement,
            WhiteId = gameDto.WhiteId,
            WhiteRating = gameDto.WhiteRating,
            BlackId = gameDto.BlackId,
            BlackRating = gameDto.BlackRating,
            Moves = gameDto.Moves,
            OpeningCode = gameDto.OpeningCode,
            OpeningMoves = gameDto.OpeningMoves,
            OpeningFullname = gameDto.OpeningFullname,
            OpeningShortname = gameDto.OpeningShortname,
            OpeningResponse = gameDto.OpeningResponse,
            OpeningVariation = gameDto.OpeningVariation
        };
        
        await _gameRepository.CreateGameAsync(game);
        
        gameDto.GameId = game.GameId;
        
        return CreatedAtAction(nameof(GetGame), new { id = game.GameId }, gameDto);
    }
    
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateGame(int id, ChessGameDto gameDto)
    {
        if (id != gameDto.GameId)
        {
            return BadRequest();
        }
        
        var game = await _gameRepository.GetGameByIdAsync(id);
        
        if (game == null)
        {
            return NotFound();
        }
        
        // Update game properties
        game.Rated = gameDto.Rated;
        game.Turns = gameDto.Turns;
        game.VictoryStatus = gameDto.VictoryStatus;
        game.Winner = gameDto.Winner;
        game.TimeIncrement = gameDto.TimeIncrement;
        game.WhiteId = gameDto.WhiteId;
        game.WhiteRating = gameDto.WhiteRating;
        game.BlackId = gameDto.BlackId;
        game.BlackRating = gameDto.BlackRating;
        game.Moves = gameDto.Moves;
        game.OpeningCode = gameDto.OpeningCode;
        game.OpeningMoves = gameDto.OpeningMoves;
        game.OpeningFullname = gameDto.OpeningFullname;
        game.OpeningShortname = gameDto.OpeningShortname;
        game.OpeningResponse = gameDto.OpeningResponse;
        game.OpeningVariation = gameDto.OpeningVariation;
        
        var result = await _gameRepository.UpdateGameAsync(game);
        
        if (result)
        {
            return NoContent();
        }
        
        return BadRequest("Failed to update game");
    }
    
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteGame(int id)
    {
        if (!await _gameRepository.GameExistsAsync(id))
        {
            return NotFound();
        }
        
        var result = await _gameRepository.DeleteGameAsync(id);
        
        if (result)
        {
            return NoContent();
        }
        
        return BadRequest("Failed to delete game");
    }
    
    private string GetQueryParams(string? sortBy, string? filterBy)
    {
        var queryParams = string.Empty;
        
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            queryParams += $"&sortBy={sortBy}";
        }
        
        if (!string.IsNullOrWhiteSpace(filterBy))
        {
            queryParams += $"&filterBy={filterBy}";
        }
        
        return queryParams;
    }
}
