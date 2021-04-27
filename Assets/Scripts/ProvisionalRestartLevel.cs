using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProvisionalRestartLevel : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag.Equals("Player")) 
        {
            other.gameObject.GetComponent<Player>().IsAlive = false;
        }
        else 
        {
            Destroy(other.gameObject);
        }
    }
}
