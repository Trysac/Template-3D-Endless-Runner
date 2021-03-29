using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{
    [SerializeField] Transform playerPosition;

    float xOffset;
    float zOffset;

    void Start()
    {
        xOffset = playerPosition.position.x - transform.position.x;
        zOffset = playerPosition.position.z - transform.position.z;
    }

    void Update()
    {
        if (FindObjectOfType<Player>() != null && FindObjectOfType<Player>().GetIsAlive()) 
        {
            transform.LookAt(playerPosition);
            transform.position = new Vector3(playerPosition.position.x -xOffset, transform.position.y, playerPosition.position.z - zOffset);
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
