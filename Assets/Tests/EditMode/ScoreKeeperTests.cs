using GauntletRun.Core;
using NUnit.Framework;

namespace GauntletRun.Core.Tests
{
    public class ScoreKeeperTests
    {
        [Test]
        public void EveryMetreIsWorthTenPoints()
        {
            var score = new ScoreKeeper();
            score.AddDistance(3);
            Assert.That(score.Distance, Is.EqualTo(3));
            Assert.That(score.Score, Is.EqualTo(30));
            Assert.That(score.Coins, Is.EqualTo(0));
            Assert.That(score.Gems, Is.EqualTo(0));
        }

        [Test]
        public void CoinsAndGemsAddTheirOwnCountAndPoints()
        {
            var score = new ScoreKeeper();
            score.AddCoins(10, 20);
            score.AddGems(1, 100);
            Assert.That(score.Coins, Is.EqualTo(10));
            Assert.That(score.Gems, Is.EqualTo(1));
            Assert.That(score.Score, Is.EqualTo(120));
            Assert.That(score.Distance, Is.EqualTo(0));
        }

        [Test]
        public void TheBestScoreOnlyGoesUp()
        {
            Assert.That(ScoreKeeper.Best(500, 300), Is.EqualTo(500));
            Assert.That(ScoreKeeper.Best(200, 300), Is.EqualTo(300));
        }
    }
}
