using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{
    [SerializeField] Transform playerPosition;

    Vector3 offSet;

    void Start()
    {
        offSet = playerPosition.position - transform.position;
    }

    void Update()
    {
        if (FindObjectOfType<Player>() != null && FindObjectOfType<Player>().IsAlive) 
        {
            Vector3 cameraPos = new Vector3(transform.position.x, transform.position.y, playerPosition.position.z + - offSet.z);
            transform.position = cameraPos;
        }
    }

    public void StopCamera() 
    { 
        //This methont is not necesary 
    }

    public void StartGameOverSecuence() 
    { 
    
    }
}
