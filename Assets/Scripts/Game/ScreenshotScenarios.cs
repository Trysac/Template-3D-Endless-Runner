using System;
using System.Collections;
using System.IO;
using GauntletRun.Core;
using UnityEngine;
using UnityEngine.UIElements;
using Screen = GauntletRun.Core.Screen;

namespace GauntletRun.Game
{
    /// <summary>
    /// Opt-in capture of README screenshots: <c>GauntletRun.exe -screenshot &lt;folder&gt; -scenario title,run,jump,slide,powers,shop,pause,gameover</c>.
    /// Without the -screenshot argument nothing is created, so normal runs and WebGL builds are
    /// unaffected. The same hook runs inside the editor (ProjectBuilder.CaptureScreenshots), which
    /// renders the camera and the HUD off screen and blends them, for machines that cannot run the player.
    /// </summary>
    public sealed class ScreenshotScenarios : MonoBehaviour
    {
        const string DefaultScenarios = "title,run,jump,slide,powers,shop,pause,gameover";
        const int Width = 1280;
        const int Height = 720;

        string folder;
        string[] scenarios;
        RenderTexture sceneTarget;
        RenderTexture uiTarget;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartWhenRequested()
        {
            string target = Argument("-screenshot");
            if (string.IsNullOrEmpty(target))
            {
                return;
            }
            var host = new GameObject(nameof(ScreenshotScenarios)).AddComponent<ScreenshotScenarios>();
            host.folder = target;
            host.scenarios = (Argument("-scenario") ?? DefaultScenarios).Split(',');
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(folder);
            var director = FindAnyObjectByType<GameDirector>();
            director.ScriptedInput = true;
            if (Application.isEditor)
            {
                RenderOffscreen(director);
            }
            yield return new WaitForSecondsRealtime(0.5f);
            foreach (string raw in scenarios)
            {
                string scenario = raw.Trim();
                UnityEngine.Random.InitState(scenario.Length * 97);
                yield return Stage(director, scenario);
                Texture2D shot;
                if (Application.isEditor)
                {
                    yield return null;
                    shot = CaptureOffscreen();
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                    shot = ScreenCapture.CaptureScreenshotAsTexture();
                }
                File.WriteAllBytes(Path.Combine(folder, scenario + ".png"), shot.EncodeToPNG());
                Destroy(shot);
                Debug.Log($"Saved screenshot {scenario}.png");
            }
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(0);
#else
            Application.Quit();
#endif
        }

        static Profile Veteran(int skin = 1) => new Profile
        {
            Best = 18450, BestDistance = 2310, Wallet = 640, Owned = 1 | 2 | 4, Skin = skin, Runs = 12,
        };

        static IEnumerator WaitUntil(Func<bool> condition, float timeout)
        {
            float waited = 0f;
            while (!condition() && waited < timeout)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        static IEnumerator Stage(GameDirector director, string scenario)
        {
            director.Autopilot = false;
            director.Frozen = false;
            switch (scenario)
            {
                case "title":
                    director.UseMemoryStore(Veteran());
                    director.ToTitle();
                    yield return new WaitForSecondsRealtime(3f);
                    break;
                case "run":
                    director.UseMemoryStore(Veteran());
                    director.StartTrial(new TrackGenerator(21));
                    director.Simulate(330f);
                    director.Autopilot = true;
                    yield return new WaitForSecondsRealtime(2.5f);
                    break;
                case "jump":
                case "slide":
                {
                    director.UseMemoryStore(Veteran(2));
                    director.StartTrial(new TrackGenerator(scenario == "jump" ? 8 : 14));
                    director.Simulate(150f);
                    director.Autopilot = true;
                    Run run = director.Session.Run;
                    if (scenario == "jump")
                    {
                        yield return WaitUntil(() => run.Runner.Y > 1.2f && run.Runner.VerticalSpeed < 1f, 30f);
                    }
                    else
                    {
                        yield return WaitUntil(() => run.Runner.Sliding && NearBeam(run), 30f);
                        yield return new WaitForSecondsRealtime(0.05f);
                    }
                    // Hold the moment still for the capture.
                    director.Frozen = true;
                    yield return null;
                    break;
                }
                case "powers":
                {
                    director.UseMemoryStore(Veteran(2));
                    director.StartTrial(new TrackGenerator(33));
                    director.Simulate(1250f);
                    Run run = director.Session.Run;
                    run.Grant(PickupKind.Shield);
                    run.Grant(PickupKind.Magnet);
                    run.Grant(PickupKind.Double);
                    director.Autopilot = true;
                    yield return new WaitForSecondsRealtime(1.6f);
                    break;
                }
                case "shop":
                    director.UseMemoryStore(new Profile { Best = 18450, Wallet = 940, Owned = 1 | 2 | 4, Skin = 2, Runs = 12 });
                    director.ToTitle();
                    yield return new WaitForSecondsRealtime(0.5f);
                    director.OpenShop();
                    yield return new WaitForSecondsRealtime(2.5f);
                    break;
                case "pause":
                    director.UseMemoryStore(Veteran());
                    director.StartTrial(new TrackGenerator(5));
                    director.Simulate(600f);
                    director.Autopilot = true;
                    yield return new WaitForSecondsRealtime(1.5f);
                    director.TogglePause();
                    yield return new WaitForSecondsRealtime(0.6f);
                    break;
                case "gameover":
                    director.UseMemoryStore(new Profile { Best = 1200, Wallet = 210, Runs = 6 });
                    director.StartTrial(new TrackGenerator(40));
                    director.Simulate(780f);
                    yield return WaitUntil(() => director.Session.Screen == Screen.GameOver, 40f);
                    yield return new WaitForSecondsRealtime(0.8f);
                    break;
                default:
                    Debug.LogWarning($"Unknown screenshot scenario '{scenario}'.");
                    break;
            }
        }

        static bool NearBeam(Run run)
        {
            foreach (Obstacle o in run.Obstacles)
            {
                float gap = o.Z - run.Distance;
                if (o.Kind == ObstacleKind.Beam && gap > 0.6f && gap < 2.6f && Mathf.Abs(Tuning.LaneX(o.Lane) - run.Runner.X) < 0.5f) return true;
            }
            return false;
        }

        /// <summary>Points the camera and the HUD panel at render textures of a fixed size.</summary>
        void RenderOffscreen(GameDirector director)
        {
            sceneTarget = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32) { antiAliasing = 4 };
            uiTarget = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            Camera.main.targetTexture = sceneTarget;
            var document = director.Hud.GetComponent<UIDocument>();
            PanelSettings panel = Instantiate(document.panelSettings);
            panel.targetTexture = uiTarget;
            panel.clearColor = true;
            panel.colorClearValue = Color.clear;
            document.panelSettings = panel;
            director.Hud.Init();
            director.RebindHud();
        }

        /// <summary>Blends the HUD over the 3D view, like the screen would show them.</summary>
        Texture2D CaptureOffscreen()
        {
            Color[] scene = Read(sceneTarget);
            Color[] ui = Read(uiTarget);
            for (int i = 0; i < scene.Length; i++)
            {
                Color over = ui[i];
                Color result = Color.Lerp(scene[i], new Color(over.r, over.g, over.b), over.a);
                result.a = 1f;
                scene[i] = result;
            }
            var shot = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
            shot.SetPixels(scene);
            shot.Apply();
            return shot;
        }

        static Color[] Read(RenderTexture target)
        {
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            var texture = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            texture.Apply();
            RenderTexture.active = previous;
            Color[] pixels = texture.GetPixels();
            Destroy(texture);
            return pixels;
        }

        static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
