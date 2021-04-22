using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("Genereal Parameters")]
    [SerializeField] float platformTravelSpeed = 2;
    [SerializeField] int platformLimitCount = 4;

    [Header("Prefabs")]
    [SerializeField] Platform[] basicPlatform;
    [SerializeField] GameObject[] MediumPlatform;
    [SerializeField] GameObject[] HardPlatform;
    [SerializeField] GameObject[] interceptionPlatform;

    int platformsInWorld;

    void Update()
    {
        if (Game_Manager.IsGameActive) 
        {
            platformsInWorld = FindObjectsOfType<Platform>().Length;
            SpawnPlatform();
        }
    }

    private void SpawnPlatform() 
    {
        if (platformsInWorld >= platformLimitCount) {return;}
        Platform platform = Instantiate(basicPlatform[0], new Vector3(0,0,44), Quaternion.identity);
        platform.TravelSpeed = platformTravelSpeed;
    }

}
