using GauntletRun.Core;
using UnityEngine;

/// <summary>Instantiates the platforms the core road adds, alternating trap and connection pieces.</summary>
public class PlatformGenerator : MonoBehaviour
{
    [Header("General Parameters")]
    [SerializeField] float platformTravelSpeed = 2;
    [SerializeField] Transform platformsContainer;

    [Header("Prefabs")]
    [SerializeField] Platform[] basicTrapsPlatforms;
    [SerializeField] Platform[] basicConnectionPlatforms;
    [SerializeField] GameObject[] MediumPlatform;
    [SerializeField] GameObject[] HardPlatform;
    [SerializeField] GameObject[] interceptionPlatform;

    void LateUpdate()
    {
        PlatformTrack track = Game_Manager.Run.Track;
        int added = Game_Manager.PlatformsThisFrame;
        for (int i = track.Platforms.Count - added; i < track.Platforms.Count; i++)
        {
            Platform[] pool = PlatformTrack.IsTrap(i) ? basicTrapsPlatforms : basicConnectionPlatforms;
            Platform platform = Instantiate(pool[Random.Range(0, pool.Length)], new Vector3(0, 0, track.Platforms[i]), Quaternion.identity);
            platform.TravelSpeed = platformTravelSpeed;
            platform.transform.SetParent(platformsContainer);
        }
    }
}
