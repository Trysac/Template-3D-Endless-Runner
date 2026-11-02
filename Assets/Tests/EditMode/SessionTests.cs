using NUnit.Framework;

namespace GauntletRun.Core.Tests
{
    public class DifficultyTests
    {
        [Test]
        public void SpeedRisesWithDistanceAndNeverPassesTheCap()
        {
            Assert.That(Difficulty.SpeedAt(0f), Is.EqualTo(Tuning.StartSpeed));
            Assert.That(Difficulty.SpeedAt(Tuning.SpeedHalfDistance), Is.EqualTo((Tuning.StartSpeed + Tuning.MaxSpeed) / 2f).Within(1e-3f));
            float previous = 0f;
            for (float d = 0f; d < 100000f; d += 250f)
            {
                float speed = Difficulty.SpeedAt(d);
                Assert.That(speed, Is.GreaterThan(previous));
                Assert.That(speed, Is.LessThan(Tuning.MaxSpeed));
                previous = speed;
            }
        }

        [Test]
        public void TiersStartAtTheirDistances()
        {
            Assert.That(Difficulty.TierAt(0f), Is.EqualTo(0));
            Assert.That(Difficulty.TierAt(299f), Is.EqualTo(0));
            Assert.That(Difficulty.TierAt(300f), Is.EqualTo(1));
            Assert.That(Difficulty.TierAt(2499f), Is.EqualTo(3));
            Assert.That(Difficulty.TierAt(99999f), Is.EqualTo(Difficulty.MaxTier));
        }

        [Test]
        public void PatternsStretchWithSpeedAndGapsShrinkWithTier()
        {
            Assert.That(Difficulty.Stretch(0f), Is.EqualTo(1f));
            Assert.That(Difficulty.Stretch(5000f), Is.GreaterThan(1.5f));
            Assert.That(Difficulty.PatternGap(4), Is.LessThan(Difficulty.PatternGap(0)));
            Assert.That(Difficulty.PatternGap(9), Is.EqualTo(Difficulty.PatternGap(Difficulty.MaxTier)));
        }
    }

    public class SessionTests
    {
        static Session NewSession(out MemoryProfileStore store, Profile start = null)
        {
            store = new MemoryProfileStore(start);
            return new Session(store);
        }

        /// <summary>A wall right ahead in the middle lane: the run ends within a couple of seconds.</summary>
        static ScriptedTrack Crash(string coins = ".c.") => new ScriptedTrack(10f, 3f, coins + "|.W.");

        static void Frames(Session session, float seconds)
        {
            for (float t = 0f; t < seconds; t += 1f / 30f)
            {
                session.Advance(1f / 30f);
            }
        }

        [Test]
        public void ANewSessionOpensOnTheTitleWithADefaultProfile()
        {
            Session session = NewSession(out _);
            Assert.That(session.Screen, Is.EqualTo(Screen.Title));
            Assert.That(session.Run, Is.Null);
            Assert.That(session.Profile.Best, Is.EqualTo(0));
            Assert.That(session.Profile.Skin, Is.EqualTo(0));
        }

        [Test]
        public void StartingARunPlaysAGeneratedRoad()
        {
            Session session = NewSession(out _);
            session.StartRun(5);
            Assert.That(session.Screen, Is.EqualTo(Screen.Playing));
            Assert.That(session.Seed, Is.EqualTo(5));
            Frames(session, 1f);
            Assert.That(session.Run.Distance, Is.EqualTo(Tuning.StartSpeed).Within(0.5f));
        }

        [Test]
        public void TheRunAdvancesInFixedStepsWhateverTheFrameRate()
        {
            Session a = NewSession(out _), b = NewSession(out _);
            a.StartRun(new ScriptedTrack(1000f, 3f, "..."));
            b.StartRun(new ScriptedTrack(1000f, 3f, "..."));
            for (int i = 0; i < 60; i++) a.Advance(1f / 60f);
            for (int i = 0; i < 20; i++) b.Advance(1f / 20f);
            Assert.That(a.Run.Distance, Is.EqualTo(b.Run.Distance).Within(0.2f));
        }

        [Test]
        public void PressesWaitForTheNextStep()
        {
            Session session = NewSession(out _);
            session.StartRun(new ScriptedTrack(1000f, 3f, "..."));
            session.Press(RunInput.Left);
            session.Advance(0.001f);
            Assert.That(session.Run.Runner.Lane, Is.EqualTo(1), "no step has run yet");
            session.Advance(1f / 60f);
            Assert.That(session.Run.Runner.Lane, Is.EqualTo(0));
        }

        [Test]
        public void PausingFreezesTheRunAndResumingContinues()
        {
            Session session = NewSession(out _);
            session.StartRun(new ScriptedTrack(1000f, 3f, "..."));
            Frames(session, 0.5f);
            session.TogglePause();
            Assert.That(session.Screen, Is.EqualTo(Screen.Paused));
            float distance = session.Run.Distance;
            session.Press(RunInput.Left);
            Frames(session, 1f);
            Assert.That(session.Run.Distance, Is.EqualTo(distance));
            Assert.That(session.Run.Runner.Lane, Is.EqualTo(1), "presses made while paused are dropped");
            session.TogglePause();
            Assert.That(session.Screen, Is.EqualTo(Screen.Playing));
            Frames(session, 0.5f);
            Assert.That(session.Run.Distance, Is.GreaterThan(distance + 4f));
        }

        [Test]
        public void ACrashShowsTheResultsAfterABeatAndPaysTheWalletOnce()
        {
            Session session = NewSession(out MemoryProfileStore store);
            session.StartRun(Crash());
            Frames(session, 2f);
            Assert.That(session.Run.Alive, Is.False);
            Assert.That(session.Dying, Is.True);
            Assert.That(session.Screen, Is.EqualTo(Screen.Playing), "the knight lies still for a beat");
            session.TogglePause();
            Assert.That(session.Screen, Is.EqualTo(Screen.Playing), "no pausing during the beat");

            Frames(session, Session.DeathBeat + 0.1f);
            Assert.That(session.Screen, Is.EqualTo(Screen.GameOver));
            RunResult result = session.LastResult;
            Assert.That(result.Coins, Is.EqualTo(1));
            Assert.That(result.Earned, Is.EqualTo(1));
            Assert.That(result.KilledBy, Is.EqualTo(ObstacleKind.Wall));
            Assert.That(result.NewBest, Is.True);
            Assert.That(result.Distance, Is.EqualTo(12), "stopped at the wall 13 m in");
            Assert.That(session.Profile.Wallet, Is.EqualTo(1));
            Assert.That(session.Profile.Best, Is.EqualTo(result.Score));
            Assert.That(session.Profile.Runs, Is.EqualTo(1));
            Assert.That(store.Load().Best, Is.EqualTo(result.Score), "saved");
            Assert.That(store.Load().Wallet, Is.EqualTo(1));

            Frames(session, 3f);
            Assert.That(session.Profile.Wallet, Is.EqualTo(1), "paid once");
        }

        [Test]
        public void AWorseRunKeepsTheBestScore()
        {
            Session session = NewSession(out _, new Profile { Best = 99999, BestDistance = 5000 });
            session.StartRun(Crash());
            Frames(session, 2f + Session.DeathBeat + 0.2f);
            Assert.That(session.LastResult.NewBest, Is.False);
            Assert.That(session.Profile.Best, Is.EqualTo(99999));
            Assert.That(session.Profile.BestDistance, Is.EqualTo(5000));
        }

        [Test]
        public void RestartingDuringTheBeatCancelsTheResults()
        {
            Session session = NewSession(out _);
            session.StartRun(3);
            session.TrackFactory = _ => Crash();
            session.StartRun(3);
            Frames(session, 2f);
            Assert.That(session.Dying, Is.True);
            session.TrackFactory = seed => new ScriptedTrack(1000f, 3f, "...");
            session.Restart();
            Frames(session, Session.DeathBeat + 0.5f);
            Assert.That(session.Screen, Is.EqualTo(Screen.Playing), "the old results never pop up");
            Assert.That(session.Run.Alive, Is.True);
            Assert.That(session.Seed, Is.EqualTo(4), "a restart plays a new road");
            Assert.That(session.Profile.Wallet, Is.EqualTo(0));
            Assert.That(session.Profile.Runs, Is.EqualTo(0));
        }

        [Test]
        public void LeavingForTheTitleDropsTheRun()
        {
            Session session = NewSession(out _);
            session.StartRun(1);
            session.TogglePause();
            session.ToTitle();
            Assert.That(session.Screen, Is.EqualTo(Screen.Title));
            Assert.That(session.Run, Is.Null);
            session.Advance(1f);
            Assert.That(session.Screen, Is.EqualTo(Screen.Title));
        }

        [Test]
        public void SettingsReturnToWhereTheyWereOpenedAndSaveTheVolumes()
        {
            Session session = NewSession(out MemoryProfileStore store);
            session.OpenSettings();
            Assert.That(session.Screen, Is.EqualTo(Screen.Settings));
            session.SetMusic(3);
            session.SetEffects(42);
            session.CloseSettings();
            Assert.That(session.Screen, Is.EqualTo(Screen.Title));
            Assert.That(store.Load().Music, Is.EqualTo(3));
            Assert.That(store.Load().Effects, Is.EqualTo(Profile.MaxVolume), "clamped");

            session.StartRun(1);
            session.TogglePause();
            session.OpenSettings();
            session.CloseSettings();
            Assert.That(session.Screen, Is.EqualTo(Screen.Paused));

            session.TogglePause();
            session.OpenSettings();
            Assert.That(session.Screen, Is.EqualTo(Screen.Playing), "settings only open from the title or the pause menu");
        }

        [Test]
        public void TheShopSellsSkinsForCoins()
        {
            Session session = NewSession(out MemoryProfileStore store, new Profile { Wallet = 400 });
            session.OpenShop();
            Assert.That(session.Screen, Is.EqualTo(Screen.Shop));
            Assert.That(session.Buy(2), Is.True);
            Assert.That(session.Profile.Wallet, Is.EqualTo(400 - Skins.All[2].Price));
            Assert.That(session.Profile.Owns(2), Is.True);
            Assert.That(session.Profile.Skin, Is.EqualTo(2), "a new skin is worn at once");
            Assert.That(store.Load().Owns(2), Is.True);

            Assert.That(session.Buy(2), Is.False, "owned already");
            Assert.That(session.Buy(5), Is.False, "too expensive");
            Assert.That(session.Profile.Wallet, Is.EqualTo(400 - Skins.All[2].Price));
            Assert.That(session.Buy(99), Is.False);

            Assert.That(session.Equip(5), Is.False, "not owned");
            Assert.That(session.Equip(0), Is.True);
            Assert.That(session.Profile.Skin, Is.EqualTo(0));
            Assert.That(store.Load().Skin, Is.EqualTo(0));
            session.CloseShop();
            Assert.That(session.Screen, Is.EqualTo(Screen.Title));
        }

        [Test]
        public void TheShopOpensFromTheResultsAndLeavesForTheTitle()
        {
            Session session = NewSession(out _);
            session.StartRun(new ScriptedTrack(10f, 3f, ".W."));
            Frames(session, 2f + Session.DeathBeat + 0.2f);
            Assert.That(session.Screen, Is.EqualTo(Screen.GameOver));
            session.OpenShop();
            Assert.That(session.Screen, Is.EqualTo(Screen.Shop));
            session.CloseShop();
            Assert.That(session.Screen, Is.EqualTo(Screen.Title));
            Assert.That(session.Run, Is.Null, "the finished run is put away");
        }
    }

    public class ProfileTests
    {
        [Test]
        public void AProfileSurvivesARoundTrip()
        {
            var profile = new Profile { Best = 1234, BestDistance = 987, Wallet = 55, Owned = 1 | 4 | 8, Skin = 3, Music = 2, Effects = 9, Runs = 12 };
            Profile back = Profile.Parse(profile.Serialize());
            Assert.That(back.Serialize(), Is.EqualTo(profile.Serialize()));
            Assert.That(back.Best, Is.EqualTo(1234));
            Assert.That(back.Owns(2), Is.True);
            Assert.That(back.Owns(1), Is.False);
            Assert.That(back.Skin, Is.EqualTo(3));
        }

        [Test]
        public void BrokenOrMissingTextFallsBackToDefaults()
        {
            Assert.That(Profile.Parse(null).Serialize(), Is.EqualTo(new Profile().Serialize()));
            Profile odd = Profile.Parse("best=abc;wallet=-5;music=99;skin=4;owned=0;nonsense;=3");
            Assert.That(odd.Best, Is.EqualTo(0));
            Assert.That(odd.Wallet, Is.EqualTo(0));
            Assert.That(odd.Music, Is.EqualTo(Profile.MaxVolume));
            Assert.That(odd.Owns(0), Is.True, "the first skin is always owned");
            Assert.That(odd.Skin, Is.EqualTo(0), "an unowned skin is not worn");
        }

        [Test]
        public void TheFirstSkinIsFree()
        {
            Assert.That(Skins.All[0].Price, Is.EqualTo(0));
            for (int i = 1; i < Skins.All.Count; i++)
            {
                Assert.That(Skins.All[i].Price, Is.GreaterThan(Skins.All[i - 1].Price));
            }
        }
    }
}
