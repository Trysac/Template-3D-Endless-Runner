using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] GemsPrefabs;
    [SerializeField] GameObject[] PowerupsPrefabs;
    [SerializeField] Transform pickupsParent;
    [Tooltip("In Seconds")][SerializeField] float spawnRate = 1f;

    BoxCollider boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        StartCoroutine(SpawnPickup());
    }


    void Update()
    {
        if (!Game_Manager.IsGameActive) 
        {
            StopCoroutine(SpawnPickup());
        }
    }

    private IEnumerator SpawnPickup() 
    {
        yield return new WaitForSeconds(spawnRate);

        if (Random.Range(0,11) <= 7) 
        {
            SpawnGems();
        }
        else 
        {
            SpawnPowers();
        }

        StartCoroutine(SpawnPickup());
    }

    private void SpawnGems() 
    {
        Vector3 spawnPos = new Vector3(
            boxCollider.center.x + Random.Range(-boxCollider.size.x / 2, boxCollider.size.x / 2),
            transform.position.y,
            boxCollider.center.z + Random.Range(-boxCollider.size.z / 2, boxCollider.size.z / 2));

        int index = Random.Range(0, GemsPrefabs.Length);
        GameObject pickup = Instantiate(GemsPrefabs[index], spawnPos, GemsPrefabs[index].transform.rotation);
        pickup.transform.SetParent(pickupsParent);
    }

    private void SpawnPowers() 
    {
        Vector3 spawnPos = new Vector3(
                boxCollider.center.x + Random.Range(-boxCollider.size.x / 2, boxCollider.size.x / 2),
                boxCollider.center.y + 1.2f,
                boxCollider.center.z + Random.Range(-boxCollider.size.z / 2, boxCollider.size.z / 2));

        int index = Random.Range(0, PowerupsPrefabs.Length);
        GameObject pickup = Instantiate(PowerupsPrefabs[index], spawnPos, PowerupsPrefabs[index].transform.rotation);
        pickup.transform.SetParent(pickupsParent);
    }


}
