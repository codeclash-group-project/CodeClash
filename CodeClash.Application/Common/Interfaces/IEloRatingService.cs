namespace CodeClash.Application.Common.Interfaces;

/// <summary>
/// Calculates ELO rating changes using the standard ELO formula (K=32).
/// </summary>
public interface IEloRatingService
{
    /// <summary>Returns (winnerNewRating, loserNewRating).</summary>
    (int WinnerNew, int LoserNew) Calculate(int winnerRating, int loserRating);

    /// <summary>Returns (player1NewRating, player2NewRating) for a draw.</summary>
    (int P1New, int P2New) CalculateDraw(int p1Rating, int p2Rating);
}
