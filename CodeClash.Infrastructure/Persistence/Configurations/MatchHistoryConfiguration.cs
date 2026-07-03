using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeClash.Infrastructure.Persistence.Configurations;

public class MatchHistoryConfiguration : IEntityTypeConfiguration<MatchHistory>
{
    public void Configure(EntityTypeBuilder<MatchHistory> builder)
    {
        builder.ToTable("MatchHistories");

        // ── Primary Key ───────────────────────────────────────────────────────
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        // ── Columns ───────────────────────────────────────────────────────────
        builder.Property(h => h.MatchId).IsRequired();
        builder.Property(h => h.UserId).IsRequired();
        builder.Property(h => h.OpponentId).IsRequired();
        builder.Property(h => h.ProblemId).IsRequired();
        builder.Property(h => h.RatingBefore).IsRequired();
        builder.Property(h => h.RatingAfter).IsRequired();
        builder.Property(h => h.PlayedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(h => h.Result)
               .IsRequired()
               .HasConversion(r => r.ToString(), s => Enum.Parse<MatchResult>(s))
               .HasMaxLength(10);

        // ── Indexes ───────────────────────────────────────────────────────────
        // Profile history lookups
        builder.HasIndex(h => new { h.UserId, h.PlayedAt })
               .HasDatabaseName("IX_MatchHistories_UserId_PlayedAt");

        // Leaderboard win/loss aggregations
        builder.HasIndex(h => new { h.UserId, h.Result })
               .HasDatabaseName("IX_MatchHistories_UserId_Result");

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(h => h.Match)
               .WithMany()
               .HasForeignKey(h => h.MatchId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.User)
               .WithMany()
               .HasForeignKey(h => h.UserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Opponent)
               .WithMany()
               .HasForeignKey(h => h.OpponentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Problem)
               .WithMany()
               .HasForeignKey(h => h.ProblemId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
