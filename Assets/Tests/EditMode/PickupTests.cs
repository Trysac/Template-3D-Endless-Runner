using NUnit.Framework;

namespace GauntletRun.Core.Tests
{
    public class PickupTests
    {
        const float Dt = Session.StepTime;

        static Run RunTo(Run run, float distance)
        {
            while (run.Alive && run.Distance < distance)
            {
                run.Step(RunInput.None, Dt);
            }
            return run;
        }

        [Test]
        public void CoinsInTheLaneAreCollectedAndScore()
        {
            Run run = RunTo(new Run(new ScriptedTrack(20f, 2f, ".c.|.c.|c.c"), 10f), 30f);
            Assert.That(run.Coins, Is.EqualTo(2), "only the coins in the middle lane");
            Assert.That(run.Score, Is.EqualTo(30 + 2 * Tuning.CoinPoints).Within(1));
            Assert.That(run.Events.FindAll(e => e.Kind == RunEventKind.Coin).Count, Is.EqualTo(2));
            Assert.That(run.Pickups.Count, Is.EqualTo(2), "taken pickups are dropped, the side coins remain");
            Assert.That(run.Pickups, Has.None.Matches<Pickup>(p => p.Taken));
        }

        [Test]
        public void AGemCountsSeparatelyAndFillsTheWallet()
        {
            Run run = RunTo(new Run(new ScriptedTrack(20f, 2f, ".c.|.g."), 10f), 30f);
            Assert.That(run.Gems, Is.EqualTo(1));
            Assert.That(run.Coins, Is.EqualTo(1));
            Assert.That(run.Earned, Is.EqualTo(1 + Tuning.GemCoins));
            Assert.That(run.Score, Is.EqualTo(30 + Tuning.CoinPoints + Tuning.GemPoints).Within(1));
        }

        [Test]
        public void CoinsAboveTheHeadNeedAJump()
        {
            // A coin at the top of the arc over a hurdle is out of reach from the ground...
            Run walking = RunTo(new Run(new HighCoin(), 10f), 30f);
            Assert.That(walking.Coins, Is.EqualTo(0));

            // ...and taken in a jump.
            var jumping = new Run(new HighCoin(), 10f);
            RunTo(jumping, 18f);
            jumping.Step(RunInput.JumpPress, Dt);
            RunTo(jumping, 30f);
            Assert.That(jumping.Coins, Is.EqualTo(1));
        }

        [Test]
        public void TheMagnetPullsCoinsFromTheOtherLanes()
        {
            var run = new Run(new ScriptedTrack(20f, 2f, "c.c|c.c|c.c"), 10f);
            run.Grant(PickupKind.Magnet);
            RunTo(run, 30f);
            Assert.That(run.Coins, Is.EqualTo(6));
            Assert.That(run.Runner.Lane, Is.EqualTo(1), "without moving");
        }

        [Test]
        public void WithoutTheMagnetCoinsInOtherLanesStay()
        {
            Run run = RunTo(new Run(new ScriptedTrack(20f, 2f, "c.c|c.c|c.c"), 10f), 30f);
            Assert.That(run.Coins, Is.EqualTo(0));
        }

        [Test]
        public void DoublePointsDoublesDistanceAndCoins()
        {
            var normal = RunTo(new Run(new ScriptedTrack(20f, 2f, ".c."), 10f), 30f);
            var doubled = new Run(new ScriptedTrack(20f, 2f, ".c."), 10f);
            doubled.Grant(PickupKind.Double);
            RunTo(doubled, 30f);
            Assert.That(doubled.Multiplier, Is.EqualTo(2));
            Assert.That(doubled.Score, Is.EqualTo(normal.Score * 2).Within(2));
            Assert.That(doubled.Coins, Is.EqualTo(normal.Coins), "coins themselves are not doubled");
        }

        [Test]
        public void PickingUpAPowerUpStartsItsTimer()
        {
            foreach (PickupKind kind in new[] { PickupKind.Shield, PickupKind.Magnet, PickupKind.Double })
            {
                Run run = RunTo(new Run(new ScriptedTrack(20f, 2f, ".*.", kind), 10f), 25f);
                Assert.That(run.Events.Exists(e => e.Kind == RunEventKind.PowerUp && e.Pickup == kind), Is.True, kind.ToString());
                switch (kind)
                {
                    case PickupKind.Shield: Assert.That(run.ShieldLeft, Is.GreaterThan(Tuning.ShieldTime - 1f)); break;
                    case PickupKind.Magnet: Assert.That(run.MagnetLeft, Is.GreaterThan(Tuning.MagnetTime - 1f)); break;
                    default: Assert.That(run.DoubleLeft, Is.GreaterThan(Tuning.DoubleTime - 1f)); break;
                }
            }
        }

        /// <summary>One coin two metres up in the middle lane.</summary>
        sealed class HighCoin : ITrackSource
        {
            bool done;

            public void Fill(float upTo, System.Collections.Generic.List<Obstacle> obstacles, System.Collections.Generic.List<Pickup> pickups)
            {
                if (!done)
                {
                    pickups.Add(new Pickup(1, PickupKind.Coin, 1, 20f, 2.3f));
                    done = true;
                }
            }
        }
    }
}
