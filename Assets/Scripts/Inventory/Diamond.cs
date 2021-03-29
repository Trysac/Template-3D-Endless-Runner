using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
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
                print("Diamond");
                isTaken = true;
                Destroy(gameObject);
            }
        }
    }
}
