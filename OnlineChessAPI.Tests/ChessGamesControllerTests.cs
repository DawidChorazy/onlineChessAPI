using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Tests.TestUtilities;

namespace OnlineChessAPI.Tests;

public class ChessGamesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ChessGamesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetGames_ReturnsSuccessStatusCode()
    {
        // Act
        var response = await _client.GetAsync("/api/ChessGames");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetGames_ReturnsPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/ChessGames?pageNumber=1&pageSize=10");
        var content = await response.Content.ReadFromJsonAsync<PagedListDto<ChessGameDto>>();

        // Assert
        Assert.NotNull(content);
        Assert.NotEmpty(content.Items);
        Assert.Equal(1, content.CurrentPage);
        Assert.True(content.PageSize > 0);
        Assert.True(content.Links.ContainsKey("self"));
    }

    [Fact]
    public async Task GetGame_WithValidId_ReturnsGame()
    {
        // Arrange
        int gameId = 1; // ID z danych testowych

        // Act
        var response = await _client.GetAsync($"/api/ChessGames/{gameId}");
        var content = await response.Content.ReadFromJsonAsync<ChessGameDto>();

        // Assert
        Assert.NotNull(content);
        Assert.Equal(gameId, content.GameId);
    }

    [Fact]
    public async Task GetGame_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        int invalidGameId = 999; // ID, które nie istnieje

        // Act
        var response = await _client.GetAsync($"/api/ChessGames/{invalidGameId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetGames_WithSorting_ReturnsSortedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/ChessGames?sortBy=turns");
        var content = await response.Content.ReadFromJsonAsync<PagedListDto<ChessGameDto>>();

        // Assert
        Assert.NotNull(content);
        Assert.NotEmpty(content.Items);
        
        // Sprawdź, czy wyniki są posortowane po liczbie tur (rosnąco)
        for (int i = 0; i < content.Items.Count() - 1; i++)
        {
            Assert.True(content.Items.ElementAt(i).Turns <= content.Items.ElementAt(i + 1).Turns);
        }
    }

    [Fact]
    public async Task GetGames_WithFiltering_ReturnsFilteredResults()
    {
        // Arrange
        var filterValue = "Mate"; // Filtruj po statusie zwycięstwa

        // Act
        var response = await _client.GetAsync($"/api/ChessGames?filterBy=victorystatus={filterValue}");
        var content = await response.Content.ReadFromJsonAsync<PagedListDto<ChessGameDto>>();

        // Assert
        Assert.NotNull(content);
        if (content.Items.Any())
        {
            // Sprawdź, czy wszystkie wyniki mają status "Mate"
            foreach (var game in content.Items)
            {
                Assert.Contains(filterValue, game.VictoryStatus);
            }
        }
    }
}
