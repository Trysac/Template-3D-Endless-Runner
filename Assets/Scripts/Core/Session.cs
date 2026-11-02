using System;

namespace GauntletRun.Core
{
    public enum Screen
    {
        Title,
        Playing,
        Paused,
        Settings,
        Shop,
        GameOver,
    }

    /// <summary>How a finished run went.</summary>
    public sealed class RunResult
    {
        public RunResult(int score, int distance, int coins, int gems, int earned, bool newBest, ObstacleKind? killedBy)
        {
            Score = score;
            Distance = distance;
            Coins = coins;
            Gems = gems;
            Earned = earned;
            NewBest = newBest;
            KilledBy = killedBy;
        }

        public int Score { get; }
        public int Distance { get; }
        public int Coins { get; }
        public int Gems { get; }
        public int Earned { get; }
        public bool NewBest { get; }
        public ObstacleKind? KilledBy { get; }
    }

    /// <summary>
    /// The screens and the run between them. The run advances in fixed 1/60 s steps whatever the
    /// frame rate. After a crash the knight lies still for a short beat before the results; that
    /// beat belongs to the run, so restarting or leaving cancels it.
    /// </summary>
    public sealed class Session
    {
        public const float StepTime = 1f / 60f;
        public const float DeathBeat = 1.2f;
        const int MaxStepsPerFrame = 8;

        readonly IProfileStore store;
        float accumulator;
        float beat;
        bool finishing;
        Screen settingsReturn = Screen.Title;
        RunInput pending;

        public Session(IProfileStore store)
        {
            this.store = store;
            Profile = store.Load();
        }

        public Profile Profile { get; }
        public Screen Screen { get; private set; } = Screen.Title;
        public Run Run { get; private set; }
        public RunResult LastResult { get; private set; }
        /// <summary>The seed of the current run; a restart plays a new road.</summary>
        public int Seed { get; private set; }
        public Func<int, ITrackSource> TrackFactory { get; set; } = seed => new TrackGenerator(seed);

        /// <summary>True between the crash and the results screen.</summary>
        public bool Dying => finishing;

        public void StartRun(int seed)
        {
            Seed = seed;
            Begin(TrackFactory(seed));
        }

        /// <summary>Starts a run on a given road (tests and staged scenes).</summary>
        public void StartRun(ITrackSource track, float? fixedSpeed = null) => Begin(track, fixedSpeed);

        void Begin(ITrackSource track, float? fixedSpeed = null)
        {
            Run = new Run(track, fixedSpeed);
            accumulator = 0f;
            beat = 0f;
            finishing = false;
            pending = default;
            Screen = Screen.Playing;
        }

        public void Restart() => StartRun(Seed + 1);

        /// <summary>Queues presses for the next step (they are not lost between fixed steps).</summary>
        public void Press(RunInput input)
        {
            if (input.Steer != 0) pending.Steer = input.Steer;
            pending.Jump |= input.Jump;
            pending.Slide |= input.Slide;
        }

        /// <summary>Advances the run by a frame's worth of fixed steps while playing.</summary>
        public void Advance(float dt)
        {
            if (Screen != Screen.Playing || Run == null)
            {
                pending = default;
                return;
            }
            if (finishing)
            {
                beat -= dt;
                if (beat <= 0f)
                {
                    Finish();
                }
                return;
            }
            accumulator = Math.Min(accumulator + dt, StepTime * MaxStepsPerFrame);
            while (accumulator >= StepTime && Run.Alive)
            {
                accumulator -= StepTime;
                Run.Step(pending, StepTime);
                pending = default;
            }
            if (!Run.Alive)
            {
                finishing = true;
                beat = DeathBeat;
            }
        }

        public void TogglePause()
        {
            if (Screen == Screen.Playing && !finishing)
            {
                Screen = Screen.Paused;
            }
            else if (Screen == Screen.Paused)
            {
                Screen = Screen.Playing;
            }
        }

        public void ToTitle()
        {
            Run = null;
            finishing = false;
            Screen = Screen.Title;
        }

        public void OpenSettings()
        {
            if (Screen == Screen.Title || Screen == Screen.Paused)
            {
                settingsReturn = Screen;
                Screen = Screen.Settings;
            }
        }

        public void CloseSettings()
        {
            if (Screen == Screen.Settings)
            {
                store.Save(Profile);
                Screen = settingsReturn;
            }
        }

        public void OpenShop()
        {
            if (Screen == Screen.Title || Screen == Screen.GameOver)
            {
                Screen = Screen.Shop;
            }
        }

        public void CloseShop()
        {
            if (Screen == Screen.Shop)
            {
                Screen = Screen.Title;
            }
        }

        public void SetMusic(int level) => Profile.Music = Math.Max(0, Math.Min(Profile.MaxVolume, level));

        public void SetEffects(int level) => Profile.Effects = Math.Max(0, Math.Min(Profile.MaxVolume, level));

        /// <summary>Buys a skin and wears it; false when it is owned already or the wallet is short.</summary>
        public bool Buy(int skin)
        {
            if (skin < 0 || skin >= Skins.All.Count || Profile.Owns(skin) || Profile.Wallet < Skins.All[skin].Price)
            {
                return false;
            }
            Profile.Wallet -= Skins.All[skin].Price;
            Profile.Owned |= 1 << skin;
            Profile.Skin = skin;
            store.Save(Profile);
            return true;
        }

        /// <summary>Wears an owned skin; false when it is not owned.</summary>
        public bool Equip(int skin)
        {
            if (skin < 0 || skin >= Skins.All.Count || !Profile.Owns(skin))
            {
                return false;
            }
            Profile.Skin = skin;
            store.Save(Profile);
            return true;
        }

        void Finish()
        {
            finishing = false;
            Run run = Run;
            bool newBest = run.Score > Profile.Best;
            Profile.Best = Math.Max(Profile.Best, run.Score);
            Profile.BestDistance = Math.Max(Profile.BestDistance, (int)run.Distance);
            Profile.Wallet += run.Earned;
            Profile.Runs++;
            store.Save(Profile);
            LastResult = new RunResult(run.Score, (int)run.Distance, run.Coins, run.Gems, run.Earned, newBest, run.KilledBy);
            Screen = Screen.GameOver;
        }
    }
}
