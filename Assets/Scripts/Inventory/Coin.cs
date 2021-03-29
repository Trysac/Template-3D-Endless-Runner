using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] int Value;
    [SerializeField] bool isTaken;

    private void Start()
    {
        isTaken = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if (!isTaken) 
            { 
                print("Coin");
                isTaken = true;
                Destroy(gameObject);
            }          
        }
    }
}
