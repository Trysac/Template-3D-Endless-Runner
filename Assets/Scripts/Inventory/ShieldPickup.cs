using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    [SerializeField] AudioClip pickupSound;
    [SerializeField] GameObject pickupEffect;
    [SerializeField] float travelSpeed = 2;

    private void Update()
    {
        if (Game_Manager.IsGameActive)
        {
            transform.position = transform.position - (Vector3.forward * travelSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player")) 
        {
            other.GetComponent<SkillSystem>().isShieldActive = true;
            Destroy(gameObject);
        }
    }
}
