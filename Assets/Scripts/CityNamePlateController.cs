using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityNamePlateController : MonoBehaviour
{
    void Start()
    {
        GameObject.FindObjectOfType<HexMap>().OnCityCreated += CreateCityNameplate;
    }
    private void OnDestroy()
    {
        GameObject.FindObjectOfType<HexMap>().OnCityCreated -= CreateCityNameplate;
    }
    public GameObject CityNameplatePrefab;

    public void CreateCityNameplate(City city, GameObject cityGO)
    {
        GameObject nameGO = (GameObject)Instantiate(CityNameplatePrefab, this.transform);
        nameGO.GetComponent<MapObjectNameplate>().MyTarget = cityGO;
        nameGO.GetComponentInChildren<CityNameplate>().MyCity = city;
    }
    void Update()
    {
        
    }
}
