using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using onlineChessAPI.Controllers;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Core.Interfaces;
using OnlineChessAPI.Core.Models;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineChessAPI.Tests.Controllers
{
    public class ChessGamesControllerTests
    {
        [Fact]
        public async Task GetGames_ReturnsPagedListOfGames()
        {
            var mockRepo = new Mock<IChessGameRepository>();

            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10 };

            var games = new List<ChessGame>
            {
                new ChessGame { GameId = 1, Winner = "white" },
                new ChessGame { GameId = 2, Winner = "black" }
            };

            var pagedList = new PagedListDto<ChessGame>(
                items: games,
                totalCount: games.Count,
                pageSize: 10,
                currentPage: 1);

            pagedList.Links.Add("self", "http://localhost/api/ChessGames?pageNumber=1&pageSize=10");

            mockRepo
                .Setup(r => r.GetGamesAsync(It.Is<PaginationDto>(p => p.PageNumber == 1 && p.PageSize == 10), null, null))
                .ReturnsAsync(pagedList);

            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost");
            httpContext.Request.PathBase = "";
            mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(httpContext);

            var controller = new ChessGamesController(mockRepo.Object, mockHttpContextAccessor.Object);
            
            var result = await controller.GetGames(paginationDto);
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            var data = Assert.IsAssignableFrom<PagedListDto<ChessGameDto>>(okResult.Value);

            Assert.Equal(2, data.Items.Count());
            Assert.Equal(1, data.CurrentPage);
            Assert.Equal(10, data.PageSize);
        }
    }
}
