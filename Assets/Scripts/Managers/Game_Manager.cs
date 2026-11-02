using GauntletRun.Core;
using UnityEngine;

/// <summary>Runs the run clock from the core and keeps the camera behind the player.</summary>
public class Game_Manager : MonoBehaviour
{
    [SerializeField] Transform playerPosition;
    [Tooltip("In seconds")] [SerializeField] float distanceUpdateRate = 1f;
    [Tooltip("In seconds")] [SerializeField] float pickupSpawnRate = 4f;
    [SerializeField] float platformTravelSpeed = 2f;

    public static RunTimeline Run { get; private set; }

    public static int Distance => Run?.Score.Distance ?? 0;
    public static int GeneralScore => Run?.Score.Score ?? 0;
    public static int Coins => Run?.Score.Coins ?? 0;
    public static int Diamonds => Run?.Score.Gems ?? 0;
    public static bool IsGameActive => Run != null && Run.Active;

    /// <summary>Platforms the road gained this frame (read by the PlatformGenerator).</summary>
    public static int PlatformsThisFrame { get; private set; }

    /// <summary>Pickups due this frame (read by the PickupSpawner).</summary>
    public static int PickupsThisFrame { get; private set; }

    Vector3 offSet;
    Player player;
    bool saved;

    void Awake()
    {
        Run = new RunTimeline(distanceUpdateRate, pickupSpawnRate, new PlatformTrack(18f, 63f, platformTravelSpeed));
        PlatformsThisFrame = 0;
        PickupsThisFrame = 0;
    }

    void Start()
    {
        offSet = playerPosition.position - transform.position;
        player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        Run.Tick(Time.deltaTime, out int platforms, out int pickups);
        PlatformsThisFrame = platforms;
        PickupsThisFrame = pickups;

        if (player.IsAlive)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, playerPosition.position.z - offSet.z);
        }
        else if (!saved)
        {
            Run.End();
            SaveHighScore();
            saved = true;
        }
    }

    static void SaveHighScore()
    {
        int best = ScoreKeeper.Best(GeneralScore, (int)PlayerPrefs.GetFloat("HighScore"));
        PlayerPrefs.SetFloat("HighScore", best);
        PlayerPrefs.SetFloat("LastScore", GeneralScore);
    }
}
