using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionManager : MonoBehaviour
{
    [SerializeField] float transitionSpeed = 1f;
    [SerializeField] float yTransitionAngul = 0f;
    [SerializeField] bool isAlreadyHappening;

    [SerializeField] Transform player;

    float currentYAngul;

    void Start()
    {
        isAlreadyHappening = false;
    }

    void Update()
    {
        
    }

    public void MakeTransition() 
    {
        currentYAngul = player.transform.rotation.y;
        //Make the chance
    }

}
