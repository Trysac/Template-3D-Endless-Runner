using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] bool isDestructible;

    public bool IsDestroy{ get; set; }

    private void Start()
    {
        IsDestroy = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Player")) 
        {
            print("Game Over");
        }
        else if (collision.gameObject.tag.Equals("Projectile")) 
        {
            print("Obstacule Destroyed");
        }
    }
       
    public void DestroyObstacle() 
    {
        if (!IsDestroy) 
        {
            IsDestroy = true;
            //Animation, sound, effects...
            Destroy(gameObject);
        }        
    }

    public bool GetIsDestructible()
    {
        return isDestructible;
    }

}
