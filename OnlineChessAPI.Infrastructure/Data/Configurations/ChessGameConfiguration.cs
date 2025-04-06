using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineChessAPI.Core.Models;

namespace OnlineChessAPI.Infrastructure.Data.Configurations;

public class ChessGameConfiguration : IEntityTypeConfiguration<ChessGame>
{
    public void Configure(EntityTypeBuilder<ChessGame> builder)
    {
        builder.HasKey(g => g.GameId);
        
        builder.Property(g => g.VictoryStatus).HasMaxLength(50);
        builder.Property(g => g.Winner).HasMaxLength(20);
        builder.Property(g => g.TimeIncrement).HasMaxLength(20);
        
        builder.Property(g => g.Moves).HasColumnType("nvarchar(max)");
        
        builder.Property(g => g.OpeningCode).HasMaxLength(10);
        builder.Property(g => g.OpeningMoves).HasMaxLength(100);
        builder.Property(g => g.OpeningFullname).HasMaxLength(100);
        builder.Property(g => g.OpeningShortname).HasMaxLength(50);
        builder.Property(g => g.OpeningResponse).HasMaxLength(50);
        builder.Property(g => g.OpeningVariation).HasMaxLength(100);
        
        // Configure relationships
        builder.HasOne(g => g.WhiteUser)
            .WithMany(u => u.WhiteGames)
            .HasForeignKey(g => g.WhiteId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasOne(g => g.BlackUser)
            .WithMany(u => u.BlackGames)
            .HasForeignKey(g => g.BlackId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
