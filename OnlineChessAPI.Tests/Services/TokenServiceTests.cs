using System;
using Microsoft.Extensions.Configuration;
using OnlineChessAPI.Core.Models;
using OnlineChessAPI.Infrastructure.Services;
using Moq;
using Xunit;

namespace OnlineChessAPI.Tests.Services
{
    public class TokenServiceTests
    {
        [Fact]
        public void CreateToken_ReturnsValidTokenString()
        {
            var inMemorySettings = new Dictionary<string, string> {
                {"JWT:Secret", "123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123123"},
                {"JWT:Issuer", "TestIssuer"},
                {"JWT:Audience", "TestAudience"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var tokenService = new TokenService(configuration);

            var user = new User
            {
                Id = 1,
                Username = "testuser"
            };
            
            var token = tokenService.CreateToken(user);
            
            Assert.False(string.IsNullOrEmpty(token));
            Assert.StartsWith("ey", token);
        }
    }
}