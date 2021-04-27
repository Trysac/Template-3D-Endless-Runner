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
    [SerializeField] Platform[] basicTrapsPlatforms;
    [SerializeField] Platform[] basicConnectionPlatforms;
    [SerializeField] GameObject[] MediumPlatform;
    [SerializeField] GameObject[] HardPlatform;
    [SerializeField] GameObject[] interceptionPlatform;

    bool spawnTrapPlatform;
    private void Start()
    {
        spawnTrapPlatform = false;
        StartCoroutine(SpawnPlatforms());
    }

    void Update()
    {
        if (!Game_Manager.IsGameActive) 
        {
            StopCoroutine(SpawnPlatforms());
        }
    }

    private IEnumerator SpawnPlatforms() 
    {
        yield return new WaitForSeconds(18/ platformTravelSpeed - 0.2f);

        if (spawnTrapPlatform) { SpawnTrapPlatform();}
            else { SpawnConnectionPlatform();}

        StartCoroutine(SpawnPlatforms());
    }

    private void SpawnTrapPlatform() 
    {
        Platform platform = Instantiate(basicTrapsPlatforms[Random.Range(0, basicTrapsPlatforms.Length)], new Vector3(0, 0, 63), Quaternion.identity);
        platform.TravelSpeed = platformTravelSpeed;
        platform.transform.SetParent(platformsContainer);
        spawnTrapPlatform = false;
    }
    private void SpawnConnectionPlatform() 
    {
        Platform platform = Instantiate(basicConnectionPlatforms[Random.Range(0, basicConnectionPlatforms.Length)], new Vector3(0, 0, 63), Quaternion.identity);
        platform.TravelSpeed = platformTravelSpeed;
        platform.transform.SetParent(platformsContainer);
        spawnTrapPlatform = true;
    }

}
