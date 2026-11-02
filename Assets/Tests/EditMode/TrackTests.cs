using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace GauntletRun.Core.Tests
{
    public class TrackTests
    {
        const float Dt = Session.StepTime;

        static (List<Obstacle> Obstacles, List<Pickup> Pickups, TrackGenerator Generator) Generate(int seed, float upTo)
        {
            var obstacles = new List<Obstacle>();
            var pickups = new List<Pickup>();
            var generator = new TrackGenerator(seed);
            generator.Fill(upTo, obstacles, pickups);
            return (obstacles, pickups, generator);
        }

        [Test]
        public void EveryPatternIsWellFormedAndLeavesALaneOpen()
        {
            foreach (Pattern pattern in Patterns.All)
            {
                Assert.That(pattern.Problem(), Is.Null, pattern.Name);
            }
            Assert.That(new Pattern("bad", 0, 3f, "WAW").Problem(), Does.Contain("blocks every lane"));
            Assert.That(new Pattern("bad", 0, 3f, "W.").Problem(), Does.Contain("lanes"));
            Assert.That(new Pattern("bad", 0, 3f, "x..").Problem(), Does.Contain("unknown"));
        }

        [Test]
        public void EveryTierHasPatternsOfItsOwn()
        {
            for (int tier = 0; tier <= Difficulty.MaxTier; tier++)
            {
                Assert.That(Patterns.All.Any(p => p.MinTier == tier), Is.True, $"tier {tier}");
            }
        }

        [Test]
        public void TheSameSeedBuildsTheSameRoad()
        {
            var a = Generate(42, 2000f);
            var b = Generate(42, 2000f);
            var c = Generate(43, 2000f);
            Assert.That(a.Generator.Laid, Is.EqualTo(b.Generator.Laid));
            Assert.That(a.Obstacles.Select(o => (o.Kind, o.Lane, o.Z)), Is.EqualTo(b.Obstacles.Select(o => (o.Kind, o.Lane, o.Z))));
            Assert.That(a.Generator.Laid, Is.Not.EqualTo(c.Generator.Laid));
        }

        [Test]
        public void TheRoadStartsClearAndPatternsFollowEachOtherWithoutOverlapping()
        {
            var road = Generate(7, 4000f);
            Assert.That(road.Obstacles.Min(o => o.Z), Is.GreaterThanOrEqualTo(Tuning.ClearStart));
            var spans = road.Generator.Spans;
            Assert.That(spans[0].Start, Is.EqualTo(Tuning.ClearStart));
            for (int i = 1; i < spans.Count; i++)
            {
                Assert.That(spans[i].Start, Is.EqualTo(spans[i - 1].End), $"pattern {i} starts where {i - 1} ends");
                Assert.That(spans[i].End, Is.GreaterThan(spans[i].Start));
            }
        }

        [Test]
        public void NoRowOfTheGeneratedRoadBlocksEveryLane()
        {
            for (int seed = 0; seed < 40; seed++)
            {
                var road = Generate(seed, 5000f);
                foreach (var row in road.Obstacles.GroupBy(o => o.Z))
                {
                    int blocked = row.Where(o => o.BlocksLane).Select(o => o.Lane).Distinct().Count();
                    Assert.That(blocked, Is.LessThan(Tuning.LaneCount), $"seed {seed} at {row.Key:0.0} m");
                }
            }
        }

        [Test]
        public void HarderPatternsOnlyAppearOnceTheirTierIsReached()
        {
            var road = Generate(3, 6000f);
            var byName = Patterns.All.ToDictionary(p => p.Name);
            for (int i = 0; i < road.Generator.Laid.Count; i++)
            {
                Pattern pattern = byName[road.Generator.Laid[i]];
                Assert.That(pattern.MinTier, Is.LessThanOrEqualTo(Difficulty.TierAt(road.Generator.Spans[i].Start)));
            }
            Assert.That(road.Generator.Laid.Any(n => byName[n].MinTier == Difficulty.MaxTier), Is.True, "the last tier shows up");
        }

        [Test]
        public void TheRoadGetsBusierAsTheTiersRise()
        {
            var road = Generate(11, 6000f);
            int Early() => road.Obstacles.Count(o => o.Z < 300f && o.BlocksLane);
            int Late() => road.Obstacles.Count(o => o.Z >= 2700f && o.Z < 3000f && o.BlocksLane);
            Assert.That(Late(), Is.GreaterThan(Early()));
        }

        [Test]
        public void TheRoadIsOnlyBuiltAFewSecondsAhead()
        {
            var run = new Run(new TrackGenerator(5), 10f);
            Assert.That(run.Obstacles.Max(o => o.Z), Is.LessThanOrEqualTo(Tuning.LookAhead + 40f));
            while (run.Distance < 600f)
            {
                run.Step(Autopilot.Decide(run), Dt);
                Assert.That(run.Alive, Is.True);
            }
            Assert.That(run.Obstacles.Min(o => o.Z), Is.GreaterThan(run.Distance - Tuning.KeepBehind - 5f), "passed obstacles are dropped");
            Assert.That(run.Obstacles.Max(o => o.Z), Is.GreaterThan(run.Distance + Tuning.LookAhead - 40f));
        }

        [Test]
        public void TheBotSurvivesTheGeneratedRoad()
        {
            for (int seed = 0; seed < 25; seed++)
            {
                var run = new Run(new TrackGenerator(seed));
                while (run.Alive && run.Distance < 3500f)
                {
                    run.Step(Autopilot.Decide(run), Dt);
                    run.Events.Clear();
                }
                Assert.That(run.Alive, Is.True, $"seed {seed}: the bot hit a {run.KilledBy} at {run.Distance:0} m");
            }
        }

        [Test]
        public void ADoNothingRunEndsEarly()
        {
            var run = new Run(new TrackGenerator(1));
            while (run.Alive && run.Distance < 3000f)
            {
                run.Step(RunInput.None, Dt);
            }
            Assert.That(run.Alive, Is.False);
            Assert.That(run.Distance, Is.LessThan(400f));
        }
    }
}
