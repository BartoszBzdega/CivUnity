using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnController : MonoBehaviour
{

    void Start()
    {
        hexMap = GameObject.FindObjectOfType<HexMap>();

    }

    HexMap hexMap;


    public GameObject EndTurnButton;
    Unit[] units;
    City[] cities;
    public void Update()
    {



    }

    public void EndTurn()
    {
        Debug.Log("EndTurn");
        if (hexMap.Units!=null) 
        {
            units = hexMap.Units;
        }
        if (hexMap.Cities != null)
        {
            cities = hexMap.Cities;
        }

        foreach (Unit u in units)
        {

            while (u.DoTurn())
            {

            }
        }
        foreach (Unit u in units)
        {
            u.ResetMovement();
        }





    }
}