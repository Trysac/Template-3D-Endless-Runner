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
            SceneManager.LoadScene(0);
        }
        else 
        {
            Destroy(other.gameObject);
        }
    }
}
