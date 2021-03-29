using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene_Manager : MonoBehaviour
{
    public void GoToGameOver(float waitTime = 0.5f) 
    {
        SceneManager.LoadScene("GameOver");
    }
    public void GoToMainMenu(float waitTime = 0.5f)
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void GoToGame(float waitTime = 0.5f)
    {
        SceneManager.LoadScene("Game");
    }
}
