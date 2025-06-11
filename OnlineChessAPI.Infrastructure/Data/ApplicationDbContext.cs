using Microsoft.EntityFrameworkCore;
using OnlineChessAPI.Core.Models;
using OnlineChessAPI.Infrastructure.Data.Configurations;

namespace OnlineChessAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    public DbSet<ChessGame> ChessGames { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new ChessGameConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new CommentConfiguration());
    }
}
