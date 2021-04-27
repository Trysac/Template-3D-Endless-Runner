using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game_Manager : MonoBehaviour
{
    [SerializeField] Transform playerPosition;
    [Tooltip("In seconds")] [SerializeField] float distanceUpdateRate = 1f;

    public static float Distance { get; set; }
    public static int GeneralScore { get; set; }
    public static int Coins { get; set; }
    public static int Diamonds { get; set; }

    Vector3 offSet;

    public static bool IsGameActive { get; set; }
    public static float DistanceUpdateRate { get; set; }

    void Start()
    {
        Distance = 0;
        GeneralScore = 0;
        Coins = 0;
        Diamonds = 0;

        DistanceUpdateRate = distanceUpdateRate;
        IsGameActive = true;
        StartCoroutine(UpdateDistanceAndScore());

        offSet = playerPosition.position - transform.position;
    }

    void Update()
    {
        if (FindObjectOfType<Player>().IsAlive) 
        {
            Vector3 cameraPos = new Vector3(transform.position.x, transform.position.y, playerPosition.position.z + - offSet.z);
            transform.position = cameraPos;
        }
        else 
        {
            IsGameActive = false;
            StopCoroutine(UpdateDistanceAndScore());
            UpdatePlayerPrefs();
        }
    }

    private void UpdatePlayerPrefs()
    {
        if (GeneralScore > PlayerPrefs.GetFloat("HighScore")) 
        { 
            PlayerPrefs.SetFloat("HighScore", GeneralScore);
        }

        PlayerPrefs.SetFloat("LastScore", GeneralScore);

        //if (Distance > PlayerPrefs.GetFloat("HighScore"))
        //{
        //    PlayerPrefs.SetFloat("HighScore", Distance);
        //}

        //PlayerPrefs.SetInt("CoinsInInventory", PlayerPrefs.GetInt("CoinsInInventory").Equals(0) ? Coins : PlayerPrefs.GetInt("CoinsInInventory") + Coins);
        //PlayerPrefs.SetInt("GemsInInventory", PlayerPrefs.GetInt("GemsInInventory").Equals(0) ? Diamonds : PlayerPrefs.GetInt("GemsInInventory") + Diamonds);
    }

    private IEnumerator UpdateDistanceAndScore() 
    {
        yield return new WaitForSeconds(DistanceUpdateRate);
        
        if (IsGameActive) 
        {
            Distance += 1;
            GeneralScore += 10;
            StartCoroutine(UpdateDistanceAndScore());
        }

    }

    public void StartGameOverSecuence() 
    { 
    
    }
}
