using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("Genereal Parameters")]
    [SerializeField] float platformTravelSpeed;
    [SerializeField] float platformLimitCount;

    [Header("Prefabs")]
    [SerializeField] GameObject[] basicPlatform;
    [SerializeField] GameObject[] MediumPlatform;
    [SerializeField] GameObject[] HardPlatform;
    [SerializeField] GameObject[] interceptionPlatform;

    int platformsInWorld;

    void Update()
    {
        platformsInWorld = FindObjectsOfType<Platform>().Length;
        if (FindObjectOfType<Player>().IsAlive) 
        {
            SpawnPlatform();
        }
        else 
        {
            StopAllPlatforms();
        }
    }

    private void SpawnPlatform() 
    {
        if (platformsInWorld <= platformLimitCount) {return;}
    }

    private void StopAllPlatforms() 
    {
        foreach (Platform platform in FindObjectsOfType<Platform>()) 
        {
            platform.StopMoving();
        }
    }

}
