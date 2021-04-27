using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1f;
    [SerializeField] int ValueInGems = 1;
    [SerializeField] int ValueInPoints = 100;
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            if (!isTaken)
            {
                audioSource.clip = pickupSound;
                audioSource.Play();
                GetComponent<MeshRenderer>().enabled = false;
                isTaken = true;
                Game_Manager.Coins += ValueInGems;
                Game_Manager.GeneralScore += ValueInPoints;
                Destroy(gameObject, 0.5f);
            }
        }
    }
}
