using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeClash.Infrastructure.Persistence.Configurations;

public class MatchArenaConfiguration : IEntityTypeConfiguration<MatchArena>
{
    public void Configure(EntityTypeBuilder<MatchArena> builder)
    {
        builder.ToTable("MatchArenas");

        // ── Primary Key ───────────────────────────────────────────────────────
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        // ── Columns ───────────────────────────────────────────────────────────
        builder.Property(m => m.PlayerOneId).IsRequired();
        builder.Property(m => m.PlayerTwoId).IsRequired();
        builder.Property(m => m.ProblemId).IsRequired();
        builder.Property(m => m.PlayerOneRatingBefore).IsRequired();
        builder.Property(m => m.PlayerTwoRatingBefore).IsRequired();
        builder.Property(m => m.PlayerOneRatingAfter);
        builder.Property(m => m.PlayerTwoRatingAfter);
        builder.Property(m => m.WinnerId);
        builder.Property(m => m.DurationMinutes).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired().HasColumnType("datetime2");
        builder.Property(m => m.StartedAt).HasColumnType("datetime2");
        builder.Property(m => m.EndedAt).HasColumnType("datetime2");

        builder.Property(m => m.Status)
               .IsRequired()
               .HasConversion(
                   s => s.ToString(),
                   s => Enum.Parse<MatchStatus>(s))
               .HasMaxLength(30);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(m => m.PlayerOneId).HasDatabaseName("IX_MatchArenas_PlayerOneId");
        builder.HasIndex(m => m.PlayerTwoId).HasDatabaseName("IX_MatchArenas_PlayerTwoId");
        builder.HasIndex(m => m.Status).HasDatabaseName("IX_MatchArenas_Status");
        builder.HasIndex(m => m.WinnerId).HasDatabaseName("IX_MatchArenas_WinnerId");

        // Composite index for "get active match for user"
        builder.HasIndex(m => new { m.PlayerOneId, m.Status }).HasDatabaseName("IX_MatchArenas_P1_Status");
        builder.HasIndex(m => new { m.PlayerTwoId, m.Status }).HasDatabaseName("IX_MatchArenas_P2_Status");

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(m => m.PlayerOne)
               .WithMany()
               .HasForeignKey(m => m.PlayerOneId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.PlayerTwo)
               .WithMany()
               .HasForeignKey(m => m.PlayerTwoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Problem)
               .WithMany()
               .HasForeignKey(m => m.ProblemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
