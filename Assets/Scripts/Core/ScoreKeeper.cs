using System;

namespace GauntletRun.Core
{
    /// <summary>The numbers of one run: distance, points, coins and gems.</summary>
    public sealed class ScoreKeeper
    {
        /// <summary>Points for every metre run.</summary>
        public const int PointsPerMetre = 10;

        public int Distance { get; private set; }
        public int Score { get; private set; }
        public int Coins { get; private set; }
        public int Gems { get; private set; }

        public void AddDistance(int metres)
        {
            Distance += metres;
            Score += metres * PointsPerMetre;
        }

        public void AddCoins(int coins, int points)
        {
            Coins += coins;
            Score += points;
        }

        public void AddGems(int gems, int points)
        {
            Gems += gems;
            Score += points;
        }

        /// <summary>The best score after a run that scored <paramref name="score"/>.</summary>
        public static int Best(int score, int previousBest) => Math.Max(score, previousBest);
    }
}
