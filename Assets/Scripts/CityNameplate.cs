using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CityNameplate : MonoBehaviour,  IPointerClickHandler
{
    public City MyCity;
    public void OnPointerClick(PointerEventData eventData)
    {
        //MapObjectNameplate monp = GetComponent<MapObjectNameplate>();

        GameObject.FindObjectOfType<MouseController>().selectedCity = MyCity;
    }


}
