using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float jumpingForce = 1f;
    //[SerializeField] float lateralMovementTransition = 0.5f;

    float[] limitsXPosition = { -2.5f, 2.5f };
   
    public bool IsAlive { get; set; }
    public bool IsTochingTheGround { get; set; }
    
    Rigidbody myRigidbody;
    Animator myAnimator;

    void Start()
    {
        IsAlive = true;
        IsTochingTheGround = true;

        myAnimator = GetComponent<Animator>();
        myRigidbody = GetComponent<Rigidbody>();
    }


    void Update()
    {
        if (IsAlive) 
        { 
            ManageInputs();
        }   
    }


    private void ManageInputs()
    {
        if (IsTochingTheGround) 
        { 
            if (Input.GetKeyDown(KeyCode.RightArrow) && transform.position.x < limitsXPosition[1])
            {
                LateralMovement(limitsXPosition[1]);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow) && transform.position.x > limitsXPosition[0])
            {
                LateralMovement(limitsXPosition[0]);
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
    }

    private void Jump() 
    {
        myRigidbody.AddRelativeForce(Vector3.up * jumpingForce, ForceMode.Impulse);
        IsTochingTheGround = false;
    }

    private void LateralMovement(float move) 
    {
        transform.Translate(move, 0, 0);
    }

    private void Swept() 
    {
        print("Swepting");
    }

    public void Dead() 
    {
        print("Dead - Game Over");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag.Equals("Ground"))
        {
            IsTochingTheGround = true;
        }
    }

}
