using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] float travelSpeed = 2f;

    public float TravelSpeed { get; set; }

    void Start()
    {
        TravelSpeed = travelSpeed;
    }

    void Update()
    {
        if (Game_Manager.IsGameActive) 
        { 
            Move();
        }
    }

    private void Move() 
    {
        transform.Translate(-Vector3.forward * TravelSpeed * Time.deltaTime);
    }
}
