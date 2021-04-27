using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Text Distance;
    [SerializeField] Text GeneralScore;
    [SerializeField] Text Coins;
    [SerializeField] Text Diamonds;

    void Update()
    {
        if (Game_Manager.IsGameActive) 
        {
            UpdateScore();
        }    
    }

    private void UpdateScore() 
    {
        Distance.text = Game_Manager.Distance.ToString();
        GeneralScore.text = Game_Manager.GeneralScore.ToString();
        Coins.text = Game_Manager.Coins.ToString();
        Diamonds.text = Game_Manager.Diamonds.ToString();
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
