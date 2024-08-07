using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class UnitSelectionPanel : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TextMeshProUGUI movement;
    MouseController mouseController;
    public GameObject CityBuildButton;

    // Start is called before the first frame update
    void Start()
    {
        mouseController = GameObject.FindObjectOfType<MouseController>();
        CityBuildButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(mouseController.selectedUnit!=null)
        {
            title.text = mouseController.selectedUnit.name;
            movement.text = string.Format("{0}/{1}", mouseController.selectedUnit.movementRemaining, mouseController.selectedUnit.movement);
        }

        if (mouseController.selectedUnit.canSettleCities && mouseController.selectedUnit.Hex.city == null)
        {
            CityBuildButton.SetActive(true);
        }
        else
        {
            CityBuildButton.SetActive(false);
        }

    }
}
