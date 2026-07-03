using CodeClash.Domain.Enums;

namespace CodeClash.Domain.Entities;

/// <summary>
/// Records a single code submission made by a player during a MatchArena.
/// Execution metrics are populated by the judge (stubbed until a real runner is wired in).
/// </summary>
public class MatchSubmission
{
    public Guid               Id               { get; private set; }
    public Guid               MatchArenaId     { get; private set; }
    public Guid               UserId           { get; private set; }
    public ProgrammingLanguage Language        { get; private set; }
    public string             SourceCode       { get; private set; } = string.Empty;
    public int                TestCasesPassed  { get; private set; }
    public int                TotalTestCases   { get; private set; }
    public long               ExecutionTimeMs  { get; private set; }
    public long               MemoryUsedMb     { get; private set; }
    public bool               IsAccepted       { get; private set; }
    public DateTime           SubmittedAt      { get; private set; }

    // Navigation
    public MatchArena? MatchArena { get; private set; }
    public User?       User       { get; private set; }

    // EF constructor
    private MatchSubmission() { }

    // ── Factory ──────────────────────────────────────────────────────────────

    public static MatchSubmission Create(
        Guid               matchArenaId,
        Guid               userId,
        ProgrammingLanguage language,
        string             sourceCode,
        int                testCasesPassed,
        int                totalTestCases,
        long               executionTimeMs,
        long               memoryUsedMb,
        bool               isAccepted)
    {
        return new MatchSubmission
        {
            Id              = Guid.NewGuid(),
            MatchArenaId    = matchArenaId,
            UserId          = userId,
            Language        = language,
            SourceCode      = sourceCode,
            TestCasesPassed = testCasesPassed,
            TotalTestCases  = totalTestCases,
            ExecutionTimeMs = executionTimeMs,
            MemoryUsedMb    = memoryUsedMb,
            IsAccepted      = isAccepted,
            SubmittedAt     = DateTime.UtcNow
        };
    }
}
