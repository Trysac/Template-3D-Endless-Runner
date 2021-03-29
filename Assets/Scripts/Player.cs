using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float MovementSpeed = 1f;
    [SerializeField] float lateralMovementTransition = 0.5f;

    bool IsAlive;
    bool IsTochingTheGround;
    float currentYAnguls;
    float[] limitsXPosition = {0,2};

    Rigidbody myRigidbody;

    void Start()
    {
        currentYAnguls = transform.rotation.y;
        IsAlive = true;
        IsTochingTheGround = true;

        myRigidbody = GetComponent<Rigidbody>();
    }


    void Update()
    {
        if (IsAlive) 
        { 
            myRigidbody.velocity = Vector3.forward * MovementSpeed;
            ManageInputs();
        }
        
    }

    public void Dead() { }
    private void ManageInputs()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) && transform.position.x < limitsXPosition[1])
        {
            LateralMovement(4);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.x > limitsXPosition[0])
        {
            LateralMovement(-4);
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) && IsTochingTheGround)
        {
            Jump();
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Swept();
        }

    }

    private void Jump() 
    {
        IsTochingTheGround = false;
        print("Jumping");
    }

    private void LateralMovement(float move) 
    {
        transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x + move, transform.position.y,transform.position.z), lateralMovementTransition);
    }

    private void Swept() 
    {
        print("Swepting");
    }

    public float[] GetLimitsXPosition() 
    {
        return limitsXPosition;
    }

    public bool GetIsAlive() 
    {
        return IsAlive;
    }
}
