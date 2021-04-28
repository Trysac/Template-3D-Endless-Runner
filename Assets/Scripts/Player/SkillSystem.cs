using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSystem : MonoBehaviour
{
    [SerializeField] GameObject shield;
    public bool isShieldActive { get; set; }

    void Start()
    {
        isShieldActive = false;
    }

    void Update()
    {
        if (Game_Manager.IsGameActive) 
        { 
            ActivateShield();
        }

    }

    public void ActivateShield() 
    {
        shield.SetActive(isShieldActive);
    }
}
