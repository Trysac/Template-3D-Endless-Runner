using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Text Distance;
    [SerializeField] Text Coins;
    [SerializeField] Text Diamonds;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void PauseGame() 
    { 
        //Pause
    }
    public void ResumeGame() 
    { 
        //Unpause
    }

    public void EndRun() 
    { 
        //Save the items and go to main menu
    }

}
