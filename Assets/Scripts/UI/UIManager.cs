using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] Text Distance;
    [SerializeField] Text GeneralScore;
    [SerializeField] Text Coins;
    [SerializeField] Text Diamonds;

    [Header("Game Over Panel")]
    [SerializeField] GameObject GameOverPanel;
    [SerializeField] Text Score;
    [SerializeField] Text HighScore;

    void Update()
    {
        if (Game_Manager.IsGameActive) 
        {
            UpdateScore();
        }
        else 
        {
            Distance.enabled = false;
            GeneralScore.enabled = false;
            Coins.enabled = false;
            Diamonds.enabled = false;

            GameOverPanel.SetActive(true);
            Score.text = Game_Manager.GeneralScore.ToString();
            HighScore.text = PlayerPrefs.GetFloat("HighScore").ToString();
        }
    }

    private void UpdateScore() 
    {
        Distance.text = Game_Manager.Distance.ToString();
        GeneralScore.text = Game_Manager.GeneralScore.ToString();
        Coins.text = Game_Manager.Coins.ToString();
        Diamonds.text = Game_Manager.Diamonds.ToString();
    }

    public void PlayAgain() 
    {
        SceneManager.LoadScene(0);
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
