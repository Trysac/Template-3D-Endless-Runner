using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    [SerializeField] float rotationSpeed = 1f;
    [SerializeField] int ValueInGems = 1;
    [SerializeField] int ValueInPoints = 100;
    [SerializeField] float travelSpeed = 2f;

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
        if (transform.position.y < -30) 
        {
            Destroy(gameObject);
        }

        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
        if (Game_Manager.IsGameActive)
        {
            Move();
        }
    }

    private void Move()
    {
        transform.position = transform.position - (Vector3.forward * travelSpeed * Time.deltaTime);
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
                GetComponent<SphereCollider>().enabled = false;
                isTaken = true;
                Game_Manager.Diamonds += ValueInGems;
                Game_Manager.GeneralScore += ValueInPoints;
                Destroy(gameObject, 0.5f);
            }
        }
    }
}
