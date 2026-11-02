using System.Collections;
using GauntletRun.Core;
using GauntletRun.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Screen = GauntletRun.Core.Screen;

namespace GauntletRun.Tests
{
    /// <summary>The game's flow in the real scene: starting, crashing, restarting and pausing.</summary>
    public class FlowTests
    {
        GameDirector director;

        IEnumerator Load(Profile profile = null)
        {
            SceneManager.LoadScene("Gauntlet");
            yield return null;
            yield return null;
            director = Object.FindAnyObjectByType<GameDirector>();
            Assert.That(director, Is.Not.Null);
            director.UseMemoryStore(profile);
            director.ScriptedInput = true;
            yield return null;
            Assert.That(director.Session.Screen, Is.EqualTo(Screen.Title));
            Assert.That(director.Hud.Showing, Is.EqualTo("Title"));
        }

        IEnumerator WaitFor(System.Func<bool> condition, float timeout, string what)
        {
            float start = Time.realtimeSinceStartup;
            while (!condition() && Time.realtimeSinceStartup - start < timeout)
            {
                yield return null;
            }
            Assert.That(condition(), Is.True, $"timed out waiting for {what}");
        }

        static ScriptedTrack WallAhead() => new ScriptedTrack(14f, 3f, ".c.|.c.|.W.");

        [UnityTest]
        public IEnumerator TheTitleRunsTheBotAndPlayStartsARun()
        {
            yield return Load();
            Assert.That(director.Session.Run, Is.Null);
            float before = director.Shown.Distance;
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(director.Shown.Distance, Is.GreaterThan(before + 2f), "the bot runs behind the title");
            Assert.That(director.World.SegmentCount, Is.GreaterThan(10), "the corridor is built around it");

            director.Play();
            yield return null;
            yield return null;
            Assert.That(director.Session.Screen, Is.EqualTo(Screen.Playing));
            Assert.That(director.Hud.Showing, Is.EqualTo("Playing"));
            Assert.That(director.Shown, Is.SameAs(director.Session.Run));
            Assert.That(director.Sound.Current, Is.Not.Null, "music plays");
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(director.Session.Run.Distance, Is.GreaterThan(3f));
            Assert.That(director.World.ObstacleCount, Is.EqualTo(director.Session.Run.Obstacles.Count), "a model for every obstacle");
            Assert.That(Vector3.Distance(director.Knight.Root.position, new Vector3(0f, 0f, director.Session.Run.Distance)), Is.LessThan(0.6f));
        }

        [UnityTest]
        public IEnumerator SteeringAndJumpingMoveTheKnight()
        {
            yield return Load();
            director.StartTrial(new ScriptedTrack(1000f, 3f, "..."), 10f);
            yield return null;
            director.Press(RunInput.Right);
            yield return new WaitForSecondsRealtime(0.3f);
            Assert.That(director.Session.Run.Runner.Lane, Is.EqualTo(2));
            Assert.That(director.Knight.Root.position.x, Is.EqualTo(Tuning.LaneX(2)).Within(0.05f));
            director.Press(RunInput.JumpPress);
            yield return new WaitForSecondsRealtime(0.2f);
            Assert.That(director.Knight.Root.position.y, Is.GreaterThan(0.8f), "the model follows the jump");
        }

        [UnityTest]
        public IEnumerator CrashingShowsTheResultsAndSavesTheRun()
        {
            yield return Load();
            director.StartTrial(WallAhead(), 10f);
            yield return WaitFor(() => !director.Session.Run.Alive, 5f, "the crash");
            Assert.That(director.Session.Screen, Is.EqualTo(Screen.Playing), "the death beat first");
            yield return WaitFor(() => director.Session.Screen == Screen.GameOver, Session.DeathBeat + 2f, "the results");
            yield return null;
            Assert.That(director.Hud.Showing, Is.EqualTo("GameOver"));
            Assert.That(director.Session.LastResult.Coins, Is.EqualTo(2));
            Assert.That(director.Session.LastResult.KilledBy, Is.EqualTo(ObstacleKind.Wall));
            Assert.That(director.Session.Profile.Wallet, Is.EqualTo(2));
            Assert.That(director.Session.Profile.Best, Is.EqualTo(director.Session.LastResult.Score));
            Assert.That(director.Knight.Root.position.z, Is.EqualTo(director.Session.Run.Distance).Within(0.01f), "the knight lies where it fell");
        }

        [UnityTest]
        public IEnumerator RestartingDuringTheDeathBeatStartsACleanRun()
        {
            yield return Load();
            director.StartTrial(WallAhead(), 10f);
            yield return WaitFor(() => director.Session.Dying, 5f, "the crash");
            Run crashed = director.Session.Run;
            director.Restart();
            yield return new WaitForSecondsRealtime(Session.DeathBeat + 0.5f);
            Assert.That(director.Session.Screen, Is.EqualTo(Screen.Playing), "the old results never pop up");
            Assert.That(director.Session.Run, Is.Not.SameAs(crashed));
            Assert.That(director.Session.Run.Alive, Is.True);
            Assert.That(director.Session.Profile.Wallet, Is.EqualTo(0), "the abandoned run paid nothing");
            Assert.That(director.Hud.Showing, Is.EqualTo("Playing"));
            Assert.That(director.Knight.Root.position.z, Is.LessThan(30f), "the knight is back near the start");
        }

        [UnityTest]
        public IEnumerator PauseFreezesTheRunAndResumeContinues()
        {
            yield return Load();
            director.StartTrial(new ScriptedTrack(1000f, 3f, "..."), 10f);
            yield return new WaitForSecondsRealtime(0.3f);
            director.TogglePause();
            yield return null;
            Assert.That(director.Session.Screen, Is.EqualTo(Screen.Paused));
            Assert.That(director.Hud.Showing, Is.EqualTo("Paused"));
            float distance = director.Session.Run.Distance;
            Vector3 knight = director.Knight.Root.position;
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.That(director.Session.Run.Distance, Is.EqualTo(distance));
            Assert.That(Vector3.Distance(director.Knight.Root.position, knight), Is.LessThan(0.01f));

            director.TogglePause();
            yield return new WaitForSecondsRealtime(0.4f);
            Assert.That(director.Session.Screen, Is.EqualTo(Screen.Playing));
            Assert.That(director.Hud.Showing, Is.EqualTo("Playing"));
            Assert.That(director.Session.Run.Distance, Is.GreaterThan(distance + 2f));
        }

        [UnityTest]
        public IEnumerator TheShopSellsAndDressesTheKnight()
        {
            yield return Load(new Profile { Wallet = 200 });
            director.OpenShop();
            yield return null;
            Assert.That(director.Hud.Showing, Is.EqualTo("Shop"));
            Assert.That(director.Session.Buy(1), Is.True);
            yield return null;
            Assert.That(director.Knight.Skin, Is.EqualTo(1), "the knight wears the new skin");
            Assert.That(director.Session.Profile.Wallet, Is.EqualTo(200 - Skins.All[1].Price));
        }
    }
}
