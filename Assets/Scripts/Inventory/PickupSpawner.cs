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
        StartCoroutine(SpawnGems());
    }


    void Update()
    {
        if (!Game_Manager.IsGameActive) 
        {
            StopCoroutine(SpawnGems());
        }
    }

    private IEnumerator SpawnGems() 
    {
        yield return new WaitForSeconds(spawnRate);

        Vector3 spawnPos = new Vector3(
            boxCollider.center.x + Random.Range(-boxCollider.size.x/2, boxCollider.size.x / 2), 
            transform.position.y, 
            boxCollider.center.z + Random.Range(-boxCollider.size.z / 2, boxCollider.size.z / 2));

        int index = Random.Range(0, GemsPrefabs.Length);
        GameObject pickup = Instantiate(GemsPrefabs[index], spawnPos, GemsPrefabs[index].transform.rotation);
        pickup.transform.SetParent(pickupsParent);

        StartCoroutine(SpawnGems());
    }


}
