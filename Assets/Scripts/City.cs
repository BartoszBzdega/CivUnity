using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class City : MapObject
{

    public City()
    {
        name = "Miasto";
    }
    BuildingJob buildingJob;


    public override void SetHex(Hex newHex)
    {
        if (Hex != null)
        {
            Hex.RemoveCity(this);
        }
        base.SetHex(newHex);
        Hex.AddCity(this);

    }



}
