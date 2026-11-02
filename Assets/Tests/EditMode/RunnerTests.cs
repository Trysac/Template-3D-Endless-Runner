using System;
using NUnit.Framework;

namespace GauntletRun.Core.Tests
{
    public class RunnerTests
    {
        const float Dt = Session.StepTime;

        static Run EmptyRun(float speed = 10f) => new Run(new ScriptedTrack(1000f, 3f, "..."), speed);

        static void Steps(Run run, int count, RunInput first = default)
        {
            run.Step(first, Dt);
            for (int i = 1; i < count; i++)
            {
                run.Step(RunInput.None, Dt);
            }
        }

        [Test]
        public void TheKnightStartsInTheMiddleLaneAndRunsForward()
        {
            Run run = EmptyRun();
            Assert.That(run.Runner.Lane, Is.EqualTo(1));
            Assert.That(run.Runner.X, Is.EqualTo(0f));
            Steps(run, 60);
            Assert.That(run.Distance, Is.EqualTo(10f).Within(0.01f));
            Assert.That(run.Score, Is.EqualTo(9).Or.EqualTo(10), "a point per metre");
        }

        [Test]
        public void SteeringMovesOneLaneAtATimeAndSlidesSideways()
        {
            Run run = EmptyRun();
            Steps(run, 1, RunInput.Left);
            Assert.That(run.Runner.Lane, Is.EqualTo(0));
            Assert.That(run.Runner.X, Is.LessThan(0f).And.GreaterThan(-Tuning.LaneWidth), "it slides, it does not teleport");
            Assert.That(run.Events.Exists(e => e.Kind == RunEventKind.LaneChange), Is.True);
            Steps(run, 30);
            Assert.That(run.Runner.X, Is.EqualTo(-Tuning.LaneWidth));
        }

        [Test]
        public void SteeringNeverLeavesTheRoad()
        {
            Run run = EmptyRun();
            for (int i = 0; i < 5; i++)
            {
                Steps(run, 20, RunInput.Right);
            }
            Assert.That(run.Runner.Lane, Is.EqualTo(2));
            Assert.That(run.Runner.X, Is.EqualTo(Tuning.LaneX(2)));
            run.Events.Clear();
            Steps(run, 1, RunInput.Right);
            Assert.That(run.Events.Exists(e => e.Kind == RunEventKind.LaneChange), Is.False, "no lane change at the edge");
        }

        [Test]
        public void AJumpRisesAboutOneAndAHalfMetresAndLands()
        {
            Run run = EmptyRun();
            Steps(run, 1, RunInput.JumpPress);
            Assert.That(run.Runner.Grounded, Is.False);
            Assert.That(run.Jumps, Is.EqualTo(1));
            float peak = 0f;
            int steps = 1;
            while (!run.Runner.Grounded && steps < 200)
            {
                run.Step(RunInput.None, Dt);
                peak = Math.Max(peak, run.Runner.Y);
                steps++;
            }
            Assert.That(peak, Is.EqualTo(1.5f).Within(0.1f));
            Assert.That(steps * Dt, Is.EqualTo(0.625f).Within(0.05f));
            Assert.That(run.Runner.Y, Is.EqualTo(0f));
            Assert.That(run.Events.Exists(e => e.Kind == RunEventKind.Land), Is.True);
        }

        [Test]
        public void NoDoubleJumps()
        {
            Run run = EmptyRun();
            Steps(run, 10, RunInput.JumpPress);
            float speedBefore = run.Runner.VerticalSpeed;
            run.Step(RunInput.JumpPress, Dt);
            Assert.That(run.Runner.VerticalSpeed, Is.LessThan(speedBefore));
            Assert.That(run.Jumps, Is.EqualTo(1));
        }

        [Test]
        public void AJumpPressedJustBeforeLandingHappensOnLanding()
        {
            Run run = EmptyRun();
            Steps(run, 1, RunInput.JumpPress);
            while (run.Runner.Y > 0.15f || run.Runner.VerticalSpeed > 0f)
            {
                run.Step(RunInput.None, Dt);
            }
            run.Step(RunInput.JumpPress, Dt);
            Steps(run, 8);
            Assert.That(run.Jumps, Is.EqualTo(2));
            Assert.That(run.Runner.Grounded, Is.False);
        }

        [Test]
        public void SlidingLowersTheKnightForAMoment()
        {
            Run run = EmptyRun();
            Steps(run, 1, RunInput.SlidePress);
            Assert.That(run.Runner.Sliding, Is.True);
            Assert.That(run.Runner.Height, Is.EqualTo(Tuning.SlideHeight));
            Steps(run, (int)(Tuning.SlideTime / Dt) + 2);
            Assert.That(run.Runner.Sliding, Is.False);
            Assert.That(run.Runner.Height, Is.EqualTo(Tuning.StandHeight));
        }

        [Test]
        public void SlidingInTheAirDropsAndSlidesOnLanding()
        {
            Run run = EmptyRun();
            Steps(run, 12, RunInput.JumpPress);
            Assert.That(run.Runner.Y, Is.GreaterThan(1f));
            Steps(run, 1, RunInput.SlidePress);
            Assert.That(run.Runner.VerticalSpeed, Is.LessThanOrEqualTo(-Tuning.DropSpeed));
            Steps(run, 8);
            Assert.That(run.Runner.Grounded, Is.True);
            Assert.That(run.Runner.Sliding, Is.True);
        }

        [Test]
        public void JumpingCancelsASlide()
        {
            Run run = EmptyRun();
            Steps(run, 5, RunInput.SlidePress);
            run.Step(RunInput.JumpPress, Dt);
            Assert.That(run.Runner.Sliding, Is.False);
            Assert.That(run.Runner.Grounded, Is.False);
        }

        [Test]
        public void MilestonesAreReportedEveryFiveHundredMetres()
        {
            Run run = EmptyRun(25f);
            int milestones = 0;
            while (run.Distance < 1010f)
            {
                run.Step(RunInput.None, Dt);
                milestones += run.Events.FindAll(e => e.Kind == RunEventKind.Milestone).Count;
                run.Events.Clear();
            }
            Assert.That(milestones, Is.EqualTo(2));
        }
    }
}
