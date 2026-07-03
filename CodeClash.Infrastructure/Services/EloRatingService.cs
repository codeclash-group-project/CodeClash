using CodeClash.Application.Common.Interfaces;

namespace CodeClash.Infrastructure.Services;

/// <summary>
/// Standard ELO rating implementation with K-factor = 32.
/// Minimum rating floor is 100 to prevent negative ratings.
/// </summary>
public class EloRatingService : IEloRatingService
{
    private const int KFactor    = 32;
    private const int RatingFloor = 100;

    public (int WinnerNew, int LoserNew) Calculate(int winnerRating, int loserRating)
    {
        double expectedWinner = ExpectedScore(winnerRating, loserRating);
        double expectedLoser  = ExpectedScore(loserRating, winnerRating);

        int winnerNew = (int)Math.Round(winnerRating + KFactor * (1.0 - expectedWinner));
        int loserNew  = (int)Math.Round(loserRating  + KFactor * (0.0 - expectedLoser));

        return (Math.Max(winnerNew, RatingFloor), Math.Max(loserNew, RatingFloor));
    }

    public (int P1New, int P2New) CalculateDraw(int p1Rating, int p2Rating)
    {
        double expectedP1 = ExpectedScore(p1Rating, p2Rating);
        double expectedP2 = ExpectedScore(p2Rating, p1Rating);

        int p1New = (int)Math.Round(p1Rating + KFactor * (0.5 - expectedP1));
        int p2New = (int)Math.Round(p2Rating + KFactor * (0.5 - expectedP2));

        return (Math.Max(p1New, RatingFloor), Math.Max(p2New, RatingFloor));
    }

    private static double ExpectedScore(int ratingA, int ratingB)
        => 1.0 / (1.0 + Math.Pow(10.0, (ratingB - ratingA) / 400.0));
}
