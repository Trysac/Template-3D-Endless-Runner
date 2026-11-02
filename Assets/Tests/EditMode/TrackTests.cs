using GauntletRun.Core;
using NUnit.Framework;

namespace GauntletRun.Core.Tests
{
    public class TrackTests
    {
        [Test]
        public void SteeringMovesInsideTheRoad()
        {
            Assert.That(LateralMovement.Step(0f, 1f, 0.5f, 2f, 2.5f), Is.EqualTo(1f));
            Assert.That(LateralMovement.Step(0f, -1f, 0.5f, 2f, 2.5f), Is.EqualTo(-1f));
            Assert.That(LateralMovement.Step(1f, 0f, 0.5f, 2f, 2.5f), Is.EqualTo(1f));
        }

        [Test]
        public void SteeringNeverLeavesTheRoad()
        {
            Assert.That(LateralMovement.Step(2.4f, 1f, 0.5f, 2f, 2.5f), Is.EqualTo(2.5f));
            Assert.That(LateralMovement.Step(-2.4f, -1f, 0.5f, 2f, 2.5f), Is.EqualTo(-2.5f));
            Assert.That(LateralMovement.Step(2.5f, 1f, 0.5f, 2f, 2.5f), Is.EqualTo(2.5f));
        }

        [Test]
        public void TheRoadAddsPlatformsFarAheadAndSlidesThemTowardsThePlayer()
        {
            var track = new PlatformTrack(18f, 63f, 2f);
            int added = track.Tick(track.Interval);
            Assert.That(added, Is.EqualTo(1));
            Assert.That(track.Platforms[0], Is.EqualTo(63f).Within(1e-3f));
            track.Tick(1f);
            Assert.That(track.Platforms[0], Is.EqualTo(61f).Within(1e-3f));
        }

        [Test]
        public void PlatformsAlternateBetweenConnectionAndTrapPieces()
        {
            Assert.That(PlatformTrack.IsTrap(0), Is.False);
            Assert.That(PlatformTrack.IsTrap(1), Is.True);
            Assert.That(PlatformTrack.IsTrap(2), Is.False);
        }

        [Test]
        public void TheRunCountsOneMetreEverySecond()
        {
            var run = new RunTimeline(1f, 4f, new PlatformTrack(18f, 63f, 2f));
            for (int i = 0; i < 10; i++)
            {
                run.Tick(0.5f, out _, out _);
            }
            Assert.That(run.Score.Distance, Is.EqualTo(5));
            Assert.That(run.Score.Score, Is.EqualTo(50));
            Assert.That(run.Active, Is.True);
        }

        [Test]
        public void ConsecutivePlatformsMeetEdgeToEdge()
        {
            foreach (float speed in new[] { 2f, 6f, 12f })
            {
                var track = new PlatformTrack(18f, 63f, speed);
                for (int i = 0; i < 4000; i++)
                {
                    track.Tick(1f / 60f);
                }
                Assert.That(track.Platforms.Count, Is.GreaterThan(3));
                for (int i = 1; i < track.Platforms.Count; i++)
                {
                    Assert.That(track.Platforms[i] - track.Platforms[i - 1], Is.EqualTo(18f).Within(1e-2f), $"gap {i} at speed {speed}");
                }
            }
        }

        [Test]
        public void NothingSpawnsAfterTheRunEnds()
        {
            var run = new RunTimeline(1f, 4f, new PlatformTrack(18f, 63f, 2f));
            run.Tick(10f, out _, out _);
            int platforms = run.Track.Platforms.Count;
            int distance = run.Score.Distance;
            run.End();
            run.Tick(20f, out int platformsAdded, out int pickupsAdded);
            Assert.That(run.Active, Is.False);
            Assert.That(platformsAdded, Is.EqualTo(0));
            Assert.That(pickupsAdded, Is.EqualTo(0));
            Assert.That(run.Track.Platforms.Count, Is.EqualTo(platforms));
            Assert.That(run.Score.Distance, Is.EqualTo(distance));
        }

        [Test]
        public void APickupIsDueEveryPickupInterval()
        {
            var run = new RunTimeline(1f, 4f, new PlatformTrack(18f, 63f, 2f));
            run.Tick(3.9f, out _, out int early);
            run.Tick(0.2f, out _, out int due);
            Assert.That(early, Is.EqualTo(0));
            Assert.That(due, Is.EqualTo(1));
        }
    }
}
