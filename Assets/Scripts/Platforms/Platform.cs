using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] float TravelSpeed;

    void Start()
    {
        
    }

    void Update()
    {
        Move();
    }

    private void Move() 
    { 
        //Move
    }

    public void StopMoving() 
    {
        TravelSpeed = 0;
    }

    public void SetTravelSpeed(float speed) 
    {
        TravelSpeed = speed;
    }

}
