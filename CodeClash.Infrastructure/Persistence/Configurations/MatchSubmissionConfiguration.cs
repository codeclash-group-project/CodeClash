using CodeClash.Domain.Entities;
using CodeClash.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CodeClash.Infrastructure.Persistence.Configurations;

public class MatchSubmissionConfiguration : IEntityTypeConfiguration<MatchSubmission>
{
    public void Configure(EntityTypeBuilder<MatchSubmission> builder)
    {
        builder.ToTable("MatchSubmissions");

        // ── Primary Key ───────────────────────────────────────────────────────
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        // ── Columns ───────────────────────────────────────────────────────────
        builder.Property(s => s.MatchArenaId).IsRequired();
        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.SourceCode).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(s => s.TestCasesPassed).IsRequired();
        builder.Property(s => s.TotalTestCases).IsRequired();
        builder.Property(s => s.ExecutionTimeMs).IsRequired();
        builder.Property(s => s.MemoryUsedMb).IsRequired();
        builder.Property(s => s.IsAccepted).IsRequired();
        builder.Property(s => s.SubmittedAt).IsRequired().HasColumnType("datetime2");

        builder.Property(s => s.Language)
               .IsRequired()
               .HasConversion(l => l.ToString(), s => Enum.Parse<ProgrammingLanguage>(s))
               .HasMaxLength(20);

        // ── Indexes ───────────────────────────────────────────────────────────
        builder.HasIndex(s => new { s.MatchArenaId, s.UserId })
               .HasDatabaseName("IX_MatchSubmissions_ArenaId_UserId");

        builder.HasIndex(s => new { s.MatchArenaId, s.IsAccepted })
               .HasDatabaseName("IX_MatchSubmissions_ArenaId_IsAccepted");

        // ── Relationships ─────────────────────────────────────────────────────
        builder.HasOne(s => s.MatchArena)
               .WithMany()
               .HasForeignKey(s => s.MatchArenaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.User)
               .WithMany()
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
