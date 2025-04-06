using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using OnlineChessAPI.Core.DTOs;
using OnlineChessAPI.Tests.TestUtilities;

namespace OnlineChessAPI.Tests;

public class AuthControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsSuccessStatusCode()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            Username = $"testuser_{Guid.NewGuid()}",
            Email = $"test_{Guid.NewGuid()}@example.com",
            Password = "Test@123",
            ConfirmPassword = "Test@123"
        };

        // Act
        var json = JsonSerializer.Serialize(registerDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", responseBody.ToLower());
    }

    [Fact]
    public async Task Register_WithInvalidData_ReturnsBadRequest()
    {
        // Arrange
        var registerDto = new UserRegisterDto
        {
            Username = "test",
            Email = "invalid-email", // Niepoprawny format email
            Password = "123", // Zbyt krótkie hasło
            ConfirmPassword = "different" // Hasła nie pasują
        };

        // Act
        var json = JsonSerializer.Serialize(registerDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/register", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange - najpierw rejestrujemy użytkownika
        var username = $"logintest_{Guid.NewGuid()}";
        var password = "Test@123";
        
        var registerDto = new UserRegisterDto
        {
            Username = username,
            Email = $"{username}@example.com",
            Password = password,
            ConfirmPassword = password
        };

        var registerJson = JsonSerializer.Serialize(registerDto);
        var registerContent = new StringContent(registerJson, Encoding.UTF8, "application/json");
        await _client.PostAsync("/api/auth/register", registerContent);

        // Teraz próbujemy się zalogować
        var loginDto = new UserLoginDto
        {
            Username = username,
            Password = password
        };

        // Act
        var loginJson = JsonSerializer.Serialize(loginDto);
        var loginContent = new StringContent(loginJson, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", loginContent);

        // Assert
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", responseBody.ToLower());
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto
        {
            Username = "nonexistent_user",
            Password = "wrong_password"
        };

        // Act
        var json = JsonSerializer.Serialize(loginDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
