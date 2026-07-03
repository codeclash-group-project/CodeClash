using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeClash.Infrastructure.Persistence.Configurations;

public class MatchmakingQueueConfiguration : IEntityTypeConfiguration<MatchmakingQueue>
{
    public void Configure(EntityTypeBuilder<MatchmakingQueue> builder)
    {
        builder.ToTable("MatchmakingQueues");

        // ── Primary Key ───────────────────────────────────────────────────────
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ValueGeneratedNever();

        // ── Columns ───────────────────────────────────────────────────────────
        builder.Property(q => q.UserId).IsRequired();
        builder.Property(q => q.Elo).IsRequired();
        builder.Property(q => q.DurationMinutes).IsRequired();
        builder.Property(q => q.IsSearching).IsRequired().HasDefaultValue(true);
        builder.Property(q => q.QueuedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(q => q.Difficulty)
               .IsRequired()
               .HasConversion(d => d.ToString(), s => Enum.Parse<Difficulty>(s))
               .HasMaxLength(20);

        builder.Property(q => q.Category)
               .IsRequired()
               .HasConversion(c => c.ToString(), s => Enum.Parse<ProblemCategory>(s))
               .HasMaxLength(30);

        builder.Property(q => q.PreferredLanguage)
               .IsRequired()
               .HasConversion(l => l.ToString(), s => Enum.Parse<ProgrammingLanguage>(s))
               .HasMaxLength(20);

        // ── Indexes ───────────────────────────────────────────────────────────
        // One active entry per user
        builder.HasIndex(q => new { q.UserId, q.IsSearching })
               .HasDatabaseName("IX_MatchmakingQueues_UserId_IsSearching");

        // Matchmaking scan: searching + difficulty + ELO
        builder.HasIndex(q => new { q.IsSearching, q.Difficulty, q.Elo })
               .HasDatabaseName("IX_MatchmakingQueues_Searching_Difficulty_Elo");

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(q => q.User)
               .WithMany()
               .HasForeignKey(q => q.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
