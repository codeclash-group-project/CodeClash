namespace CodeClash.Domain.Enums;

public enum MatchStatus
{
    WaitingForOpponent = 1,
    Countdown          = 2,
    InProgress         = 3,
    Completed          = 4,
    Cancelled          = 5,
    Disconnected       = 6
}
