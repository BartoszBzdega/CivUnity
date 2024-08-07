using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildCityButton : MonoBehaviour
{

public void BuildCity()
    {
        City city = new City();
        HexMap map = GameObject.FindObjectOfType<HexMap>();
        MouseController mouseController = GameObject.FindObjectOfType<MouseController>();
        map.SpawnCityAt(city, map.CityPrefab, mouseController.selectedUnit.Hex.Q, mouseController.selectedUnit.Hex.R);
    }
}
