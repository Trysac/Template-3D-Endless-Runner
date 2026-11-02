using System;
using NUnit.Framework;

namespace GauntletRun.Core.Tests
{
    public class CollisionTests
    {
        const float Dt = Session.StepTime;

        /// <summary>Runs until the knight is past <paramref name="z"/> or dead, pressing at the given distance.</summary>
        static Run RunPast(string rows, float z, Func<Run, RunInput> pilot, float speed = 10f, float axePhase = 0f)
        {
            var run = new Run(new ScriptedTrack(z, 3f, rows, axePhase: axePhase), speed);
            while (run.Alive && run.Distance < z + 6f)
            {
                run.Step(pilot(run), Dt);
            }
            return run;
        }

        static Func<Run, RunInput> PressAt(float distance, RunInput press)
        {
            bool done = false;
            return run =>
            {
                if (done || run.Distance < distance)
                {
                    return RunInput.None;
                }
                done = true;
                return press;
            };
        }

        [Test]
        public void RunningIntoAHurdleEndsTheRun()
        {
            Run run = RunPast(".H.", 20f, _ => RunInput.None);
            Assert.That(run.Alive, Is.False);
            Assert.That(run.KilledBy, Is.EqualTo(ObstacleKind.Hurdle));
            Assert.That(run.Distance, Is.EqualTo(20f - 0.25f - 0.3f).Within(0.2f), "stopped at the hurdle");
            Assert.That(run.Events.Exists(e => e.Kind == RunEventKind.Death), Is.True);
        }

        [Test]
        public void JumpingClearsAHurdle()
        {
            Run run = RunPast(".H.", 20f, PressAt(18f, RunInput.JumpPress));
            Assert.That(run.Alive, Is.True);
        }

        [Test]
        public void JumpingDoesNotClearABeam()
        {
            Run run = RunPast(".B.", 20f, PressAt(18f, RunInput.JumpPress));
            Assert.That(run.Alive, Is.False);
            Assert.That(run.KilledBy, Is.EqualTo(ObstacleKind.Beam));
        }

        [Test]
        public void SlidingPassesUnderABeam()
        {
            Run run = RunPast(".B.", 20f, PressAt(18.5f, RunInput.SlidePress));
            Assert.That(run.Alive, Is.True);
        }

        [Test]
        public void NeitherJumpNorSlideGetsPastAWall()
        {
            Assert.That(RunPast(".W.", 20f, PressAt(18f, RunInput.JumpPress)).KilledBy, Is.EqualTo(ObstacleKind.Wall));
            Assert.That(RunPast(".W.", 20f, PressAt(18.5f, RunInput.SlidePress)).KilledBy, Is.EqualTo(ObstacleKind.Wall));
        }

        [Test]
        public void ChangingLanesAvoidsAWall()
        {
            Run run = RunPast(".W.", 20f, PressAt(14f, RunInput.Left));
            Assert.That(run.Alive, Is.True);
            Assert.That(run.Runner.Lane, Is.EqualTo(0));
        }

        [Test]
        public void ObstaclesInOtherLanesAreHarmless()
        {
            Run run = RunPast("WHW", 20f, PressAt(18f, RunInput.JumpPress));
            Assert.That(run.Alive, Is.True);
        }

        [Test]
        public void AnAxeHangingLowStrikesAndOneSwungHighLetsTheKnightThrough()
        {
            // The phase sets where in its swing the axe is when the knight arrives (2 s in).
            float arrival = 20f / 10f;
            float low = (float)(-arrival * 2 * Math.PI / Run.AxePeriod);
            float high = low + (float)(Math.PI / 2);
            Assert.That(RunPast(".A.", 20f, _ => RunInput.None, axePhase: low).KilledBy, Is.EqualTo(ObstacleKind.Axe));
            Assert.That(RunPast(".A.", 20f, _ => RunInput.None, axePhase: high).Alive, Is.True);
        }

        [Test]
        public void AShieldSmashesOneObstacleThenBreaks()
        {
            var run = new Run(new ScriptedTrack(20f, 3f, ".W.|...|.W."), 10f);
            run.Grant(PickupKind.Shield);
            while (run.Alive && run.Distance < 22f)
            {
                run.Step(RunInput.None, Dt);
            }
            Assert.That(run.Alive, Is.True);
            Assert.That(run.Shielded, Is.False);
            Assert.That(run.Obstacles[0].Smashed, Is.True);
            Assert.That(run.Events.Exists(e => e.Kind == RunEventKind.ShieldBreak), Is.True);
            while (run.Alive && run.Distance < 30f)
            {
                run.Step(RunInput.None, Dt);
            }
            Assert.That(run.Alive, Is.False, "the second wall still counts");
        }

        [Test]
        public void AShieldWearsOffAfterItsTime()
        {
            var run = new Run(new ScriptedTrack(1000f, 3f, "..."), 10f);
            run.Grant(PickupKind.Shield);
            for (float t = 0f; t < Tuning.ShieldTime + 0.1f; t += Dt)
            {
                run.Step(RunInput.None, Dt);
            }
            Assert.That(run.Shielded, Is.False);
        }

        [Test]
        public void AFinishedRunStandsStill()
        {
            Run run = RunPast(".W.", 20f, _ => RunInput.None);
            float distance = run.Distance;
            int score = run.Score;
            for (int i = 0; i < 120; i++)
            {
                run.Step(RunInput.Left, Dt);
            }
            Assert.That(run.Distance, Is.EqualTo(distance));
            Assert.That(run.Score, Is.EqualTo(score));
            Assert.That(run.Runner.Lane, Is.EqualTo(1));
        }
    }
}
