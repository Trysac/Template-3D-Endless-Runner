using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1f;
    [SerializeField] int ValueInCoins = 10;
    [SerializeField] int ValueInPoints = 20;
    [SerializeField] AudioClip pickupSound;

    bool isTaken;

    AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        isTaken = false;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (!isTaken) 
            {
                audioSource.clip = pickupSound;
                audioSource.Play();
                GetComponent<MeshRenderer>().enabled = false;
                isTaken = true;
                Game_Manager.Coins += ValueInCoins;
                Game_Manager.GeneralScore += ValueInPoints;
                Destroy(gameObject, 0.5f);
            }          
        }
    }
}
