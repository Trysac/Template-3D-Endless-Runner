using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("Genereal Parameters")]
    [SerializeField] float platformTravelSpeed = 2;
    [SerializeField] int platformLimitCount = 4;
    [SerializeField] Transform platformsContainer;

    [Header("Prefabs")]
    [SerializeField] Platform[] basicPlatform;
    [SerializeField] GameObject[] MediumPlatform;
    [SerializeField] GameObject[] HardPlatform;
    [SerializeField] GameObject[] interceptionPlatform;

    //int platformsInWorld;
    private void Start()
    {
        StartCoroutine(SpawnPlatforms());
    }

    void Update()
    {
        if (Game_Manager.IsGameActive) 
        {
            //platformsInWorld = FindObjectsOfType<Platform>().Length;
            //SpawnPlatform();
        }
        else 
        {
            StopCoroutine(SpawnPlatforms());
        }
    }

    private void SpawnPlatform() 
    {
        //if (platformsInWorld >= platformLimitCount) {return;}
        Platform platform = Instantiate(basicPlatform[0], new Vector3(0,0,44), Quaternion.identity);
        platform.TravelSpeed = platformTravelSpeed;
    }

    private IEnumerator SpawnPlatforms() 
    {
        //if (platformsInWorld >= platformLimitCount) { yield return null; }
        yield return new WaitForSeconds(18/ platformTravelSpeed - 0.2f);
        Platform platform = Instantiate(basicPlatform[0], new Vector3(0, 0, 63), Quaternion.identity);
        platform.TravelSpeed = platformTravelSpeed;
        platform.transform.SetParent(platformsContainer);
        StartCoroutine(SpawnPlatforms());
    }

}
