using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] obstaculePrefabs;
    [SerializeField] Transform obstaculesParent;
    [SerializeField] float maxObstacules = 5;
    [SerializeField] float spawnDelay = 5f;

    BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        SpawnFirtsObstacules();
    }

    private void SpawnFirtsObstacules() 
    {
        for (int i = 0; i <= maxObstacules; i++) 
        {
            float xPlatformPos = transform.position.x;
            float zPlatformPos = transform.position.z;
            Vector3 position = new Vector3(xPlatformPos + Random.Range(-boxCollider.size.x/2, boxCollider.size.x/2), boxCollider.center.y -  2, zPlatformPos + Random.Range(-boxCollider.size.z/2, boxCollider.size.z/2));
            GameObject obstacle = Instantiate(obstaculePrefabs[Random.Range(0, obstaculePrefabs.Length)], position, Quaternion.identity);
            obstacle.transform.SetParent(obstaculesParent);
            obstacle.transform.Rotate(Vector3.up * Random.Range(-90, 75));
        }

        StartCoroutine(ContinuesSpawning());
    }

    private IEnumerator ContinuesSpawning() 
    {
        print("Local Spawner");
        yield return new WaitForSeconds(spawnDelay);
    }
}
