using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [Header("Objects in inventory")]
    [SerializeField] int weapond;
    [SerializeField] int watchs;
    [SerializeField] int coins;
    [SerializeField] int diamonds;

    [Header("Parameters for watch")]
    [SerializeField] float duration;
    [SerializeField] float slowDownRate;
    [SerializeField] bool used;

    [Header("Parameters for Weapon")]
    [SerializeField] int ammo;
    [SerializeField] float cooldownTime;


    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SlowDownTime() 
    { 
    
    }

    public void UseWeapon()
    {

    }

    public void SaveItems() 
    {
        //Problably this is not the place for this
    }

}
