using CodeClash.Application.Common.Interfaces;
using CodeClash.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace CodeClash.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Auth ──────────────────────────────────────────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ── Problems ──────────────────────────────────────────────────────────────
    public DbSet<Problem> Problems => Set<Problem>();
    public DbSet<TestCase> TestCases => Set<TestCase>();

    // ── Matchmaking Arena ─────────────────────────────────────────────────────
    public DbSet<MatchArena>       MatchArenas       => Set<MatchArena>();
    public DbSet<MatchmakingQueue> MatchmakingQueues => Set<MatchmakingQueue>();
    public DbSet<MatchSubmission>  MatchSubmissions  => Set<MatchSubmission>();
    public DbSet<MatchHistory>     MatchHistories    => Set<MatchHistory>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}