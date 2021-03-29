using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : MonoBehaviour
{
    [SerializeField] Text LastTotalDistance;
    [SerializeField] Text BestDistance;
    [SerializeField] Text TotalCoinsObtained;
    [SerializeField] Text TootalDiamondsObtained;

    Scene_Manager manager;

    void Start()
    {
        manager = FindObjectOfType<Scene_Manager>();
        //Connect with player prefabs
        SaveItems();
    }
    void Update()
    {
        
    }

    public void SaveItems() 
    { 
        //Player Preference
    }
    public void PlayAgain() 
    {
        manager.GoToGame();
    }
    public void ReturnToMainMenu() 
    {
        manager.GoToMainMenu();
    }

}
