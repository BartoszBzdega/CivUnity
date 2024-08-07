using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{
    void Start()
    {
        Update_CurrentFunc = Update_DetectModeStart;
        hexMap=GameObject.FindObjectOfType<HexMap>();
        lineRenderer = transform.GetComponent<LineRenderer>();
    }
    LineRenderer lineRenderer;
    Vector3 cameraTargetOffset;
    HexMap hexMap;
    Hex hexUnderMouse;
    Hex hexLastUnderMouse;
    Vector3 lastMousePosition;
    Unit __selectedUnit = null;
    public Unit selectedUnit {  get { return __selectedUnit; } 
        set {
            __selectedUnit = null;
            if(__selectedCity != null)
            {
                selectedCity = null;
            }
            __selectedUnit = value; 
            UnitSelectionPanel.SetActive(__selectedUnit!=null); 
        } 
    }
    City __selectedCity = null;
    public City selectedCity { 
        get { return __selectedCity; } 
        set {
            __selectedCity = null;
            if (__selectedUnit != null)
            {
                selectedUnit = null;
            }
            __selectedCity = value; 
            CancelUpdateFunc();
            selectedUnit = null;
            CitySelectionPanel.SetActive(__selectedCity != null);
            Update_CurrentFunc = Update_CityView;
        } }

    int mouseDragThreshold = 1; 
    Vector3 lastMouseGroundPlanePosition;
    delegate void UpdateFunc();
    UpdateFunc Update_CurrentFunc;
    public LayerMask LayerIdForHexTiles;
    Hex[] hexPath;
    private void Update()
    {
        hexUnderMouse = MouseToHex();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            selectedUnit = null;
            CancelUpdateFunc();
        }
        Update_CurrentFunc();
        Update_ScrollZoom();
        lastMousePosition = Input.mousePosition;
        hexLastUnderMouse = hexUnderMouse;
        if(selectedUnit != null)
        {
            DrawPath((hexPath!=null)?hexPath:selectedUnit.GetHexPath());
        }
        else
        {
            lineRenderer.enabled = false;
        }

    }
    public GameObject UnitSelectionPanel;
    public GameObject CitySelectionPanel;

    void Update_DetectModeStart()
    {
        if(EventSystem.current.IsPointerOverGameObject()){
            return;
        }


        if(Input.GetMouseButtonDown(0)) 
        { 

        }
        else if(Input.GetMouseButtonUp(0))
        {
            Debug.Log("puszczono przycisk myszy");
            Unit[] us = hexUnderMouse.Units;
            if (us.Length > 0 )
            {
                selectedUnit = us[0];

            }

        }
        else if (selectedUnit!=null&&Input.GetMouseButtonDown(1))
        {
            Update_CurrentFunc = Update_UnitMovement;
        }
        else if (Input.GetMouseButton(0)&&Vector3.Distance(Input.mousePosition,lastMousePosition)>mouseDragThreshold)
        {
            Update_CurrentFunc = Update_CameraDrag;
            lastMouseGroundPlanePosition = MouseToGroundPlane(Input.mousePosition);
            Update_CurrentFunc();
        }
        else if(selectedUnit!=null&&Input.GetMouseButton(1))
        {

        }
    }
    public void CancelUpdateFunc()
    {
        Update_CurrentFunc = Update_DetectModeStart;
        hexPath = null;

    }
    Hex MouseToHex()
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        int layerMask = LayerIdForHexTiles.value;

        if(Physics.Raycast(mouseRay, out hitInfo, Mathf.Infinity, layerMask))
        {
            Debug.Log(hitInfo.collider.name);
            GameObject HexGo = hitInfo.rigidbody.gameObject;
            return hexMap.GetHexFromGo(HexGo);
        }
        return null;
    }
    Vector3 MouseToGroundPlane(Vector3 mousePos)
    {
        Ray mouseRay = Camera.main.ScreenPointToRay(mousePos);
        if (mouseRay.direction.y >= 0)
        {
            return Vector3.zero;
        }
        float rayLength = (mouseRay.origin.y / mouseRay.direction.y);
        return mouseRay.origin - (mouseRay.direction * rayLength);
    }



    void Update_UnitMovement()
    {
        if (Input.GetMouseButtonUp(1) || selectedUnit==null)
        {
            if (selectedUnit != null)
            {
                selectedUnit.setHexPath(hexPath);
                StartCoroutine(hexMap.DoUnitMoves(selectedUnit));
            }
            CancelUpdateFunc();
            return;
        }
        if (hexPath == null || hexUnderMouse != hexLastUnderMouse)
        {
           hexPath= Pathing.Path.FindPath<Hex>(hexMap, selectedUnit, selectedUnit.Hex, hexUnderMouse, Hex.CostEstimate);
           DrawPath(hexPath);
        }

    }

    void DrawPath(Hex[] hexPath)
    {
        if (hexPath.Length == 0 || hexPath == null)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled= true;
        
        Vector3[] Positions = new Vector3[hexPath.Length];
        for(int i = 0;i<hexPath.Length;i++)
        { 
            GameObject hexGo = hexMap.GetHexGo(hexPath[i]);
            Positions[i] = hexGo.transform.position+(Vector3.up*0.1f);

        }
        lineRenderer.positionCount = Positions.Length;
        lineRenderer.SetPositions(Positions);
    }

    void Update_CameraDrag()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Cancelling camera drag.");
            CancelUpdateFunc();
            return;
        }



        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);

        Vector3 diff = lastMouseGroundPlanePosition - hitPos;
        Camera.main.transform.Translate(diff, Space.World);

        lastMouseGroundPlanePosition = hitPos = MouseToGroundPlane(Input.mousePosition);



    }
    void Update_ScrollZoom()
    {

        float scrollAmount = Input.GetAxis("Mouse ScrollWheel");
        float minHeight = 2;
        float maxHeight = 20;

        Vector3 hitPos = MouseToGroundPlane(Input.mousePosition);
        Vector3 dir = hitPos - Camera.main.transform.position;

        Vector3 p = Camera.main.transform.position;


        if (scrollAmount > 0 || p.y < (maxHeight - 0.1f))
        {
            cameraTargetOffset += dir * scrollAmount;
        }
        Vector3 lastCameraPosition = Camera.main.transform.position;
        Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, Camera.main.transform.position + cameraTargetOffset, Time.deltaTime * 5f);
        cameraTargetOffset -= Camera.main.transform.position - lastCameraPosition;


        p = Camera.main.transform.position;
        if (p.y < minHeight)
        {
            p.y = minHeight;
        }
        if (p.y > maxHeight)
        {
            p.y = maxHeight;
        }
        Camera.main.transform.position = p;


        Camera.main.transform.rotation = Quaternion.Euler(
            Mathf.Lerp(30, 75, Camera.main.transform.position.y / maxHeight),
            Camera.main.transform.rotation.eulerAngles.y,
            Camera.main.transform.rotation.eulerAngles.z
        );

         
    }
    public void StartCityView()
    {
        Update_CurrentFunc = Update_CityView;
    }

    void Update_CityView()
    {
        Update_DetectModeStart();
    }


}
