using Moq;
using OnlineChessAPI.Core.Interfaces;
using OnlineChessAPI.Core.Models;
using System.Threading.Tasks;
using Xunit;

public class ChessGameRepositoryTests
{
    [Fact]
    public async Task GetGameByIdAsync_ReturnsGame_WhenGameExists()
    {
        var mockRepo = new Mock<IChessGameRepository>();
        var game = new ChessGame { GameId = 1, Winner = "white" };

        mockRepo.Setup(r => r.GetGameByIdAsync(1))
            .ReturnsAsync(game);
        
        var result = await mockRepo.Object.GetGameByIdAsync(1);
        
        Assert.NotNull(result);
        Assert.Equal(1, result.GameId);
        Assert.Equal("white", result.Winner);
    }

    [Fact]
    public async Task GetGameByIdAsync_ReturnsNull_WhenGameDoesNotExist()
    {
        var mockRepo = new Mock<IChessGameRepository>();

        mockRepo.Setup(r => r.GetGameByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ChessGame?)null);
        
        var result = await mockRepo.Object.GetGameByIdAsync(99);
        
        Assert.Null(result);
    }
}