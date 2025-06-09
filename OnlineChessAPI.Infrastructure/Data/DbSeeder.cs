using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OnlineChessAPI.Core.Models;
using System.Globalization;

namespace OnlineChessAPI.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();
        
        try
        {
            logger.LogInformation("Starting database seeding...");
            
            // Upewnij się, że baza danych jest utworzona
            await context.Database.EnsureCreatedAsync();
            
            // Seeduj dane w odpowiedniej kolejności
            await SeedUsersAsync(context, logger);
            await SeedChessGamesFromCsvAsync(context, logger);
            await SeedCommentsAsync(context, logger);
            
            logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
    
    private static async Task SeedUsersAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Users already exist in the database.");
            return;
        }
        
        logger.LogInformation("Seeding users...");
        
        var users = new List<User>
        {
            new User
            {
                Username = "user1",
                Email = "user1@example.com",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 }
            },
            new User
            {
                Username = "user2",
                Email = "user2@example.com",
                PasswordHash = new byte[] { 1, 2, 3 },
                PasswordSalt = new byte[] { 4, 5, 6 }
            }
        };
        
        await context.Users.AddRangeAsync(users);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {users.Count} users to the database.");
    }
    
    private static async Task SeedChessGamesFromCsvAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.ChessGames.AnyAsync())
        {
            logger.LogInformation("Chess games already exist in the database.");
            return;
        }
        
        logger.LogInformation("Starting to seed chess games from CSV file...");
        
        var csvFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "chess_games.csv");
        
        if (!File.Exists(csvFilePath))
        {
            // Próbuj znaleźć plik w różnych lokalizacjach
            logger.LogWarning($"CSV file not found at: {csvFilePath}, trying alternative path");
            csvFilePath = "c:\\Users\\szymo\\RiderProjects\\onlineChessAPI\\chess_games.csv";
            
            if (!File.Exists(csvFilePath))
            {
                logger.LogError("CSV file not found. Cannot seed chess games.");
                return;
            }
        }
        
        logger.LogInformation($"Found CSV file at: {csvFilePath}");
        
        // Pobierz użytkowników do powiązania z grami
        var users = await context.Users.ToListAsync();
        if (users.Count < 2)
        {
            logger.LogWarning("Not enough users in the database. Make sure users are seeded first.");
            return;
        }
        
        var games = new List<ChessGame>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            Delimiter = ",", // Używamy przecinka jako separatora (standardowy format CSV)
        };
        
        try
        {
            using (var reader = new StreamReader(csvFilePath))
            using (var csv = new CsvReader(reader, config))
            {
                // Przeczytaj nagłówek
                csv.Read();
                csv.ReadHeader();
                
                int counter = 0;
                int maxRecords = int.MaxValue;
                
                while (csv.Read() && counter < maxRecords)
                {
                    try
                    {
                        var game = new ChessGame
                        {
                            // Użyj TryGet, aby obsłużyć potencjalne braki danych
                            GameId = csv.TryGetField<int>("game_id", out var gameId) ? gameId : counter + 1,
                            Rated = csv.TryGetField<bool>("rated", out var rated) ? rated : false,
                            Turns = csv.TryGetField<int>("turns", out var turns) ? turns : 0,
                            VictoryStatus = csv.GetField<string>("victory_status"),
                            Winner = csv.GetField<string>("winner"),
                            TimeIncrement = csv.GetField<string>("time_increment"),
                            // Przypisz użytkowników na podstawie dostępnych użytkowników w bazie danych
                            WhiteId = users[0].Id,
                            WhiteRating = csv.TryGetField<int>("white_rating", out var whiteRating) ? whiteRating : 1500,
                            BlackId = users[1].Id,
                            BlackRating = csv.TryGetField<int>("black_rating", out var blackRating) ? blackRating : 1500,
                            Moves = csv.GetField<string>("moves"),
                            OpeningCode = csv.GetField<string>("opening_code"),
                            OpeningMoves = csv.GetField<string>("opening_moves"),
                            OpeningFullname = csv.GetField<string>("opening_fullname"),
                            OpeningShortname = csv.GetField<string>("opening_shortname"),
                            OpeningResponse = csv.GetField<string>("opening_response"),
                            OpeningVariation = csv.GetField<string>("opening_variation")
                        };
                        
                        games.Add(game);
                        counter++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Error parsing record {counter} from CSV. Skipping record.");
                    }
                }
            }
            
            if (games.Count > 0)
            {
                await context.ChessGames.AddRangeAsync(games);
                await context.SaveChangesAsync();
                logger.LogInformation($"Successfully added {games.Count} chess games to the database.");
            }
            else
            {
                logger.LogWarning("No chess games were loaded from the CSV file.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while reading the CSV file.");
        }
    }
    
    private static async Task SeedCommentsAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Comments.AnyAsync())
        {
            logger.LogInformation("Comments already exist in the database.");
            return;
        }
        
        logger.LogInformation("Seeding comments...");
        
        var users = await context.Users.ToListAsync();
        var games = await context.ChessGames.Take(5).ToListAsync();
        
        if (!games.Any())
        {
            logger.LogWarning("No chess games found to add comments to.");
            return;
        }
        
        var comments = new List<Comment>();
        
        foreach (var game in games)
        {
            // Dodaj komentarz od zalogowanego użytkownika
            if (users.Any())
            {
                comments.Add(new Comment
                {
                    GameId = game.GameId,
                    UserId = users[0].Id,
                    Content = $"Great game! The {game.OpeningFullname} is always interesting.",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                });
            }
            
            // Dodaj anonimowy komentarz
            comments.Add(new Comment
            {
                GameId = game.GameId,
                UserId = null,
                Content = $"Anonymous comment: Interesting moves in this {game.OpeningShortname}!",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            });
        }
        
        await context.Comments.AddRangeAsync(comments);
        await context.SaveChangesAsync();
        logger.LogInformation($"Added {comments.Count} comments to the database.");
    }
}
