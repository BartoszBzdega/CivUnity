using Pathing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HexMap : MonoBehaviour, WorldPath
{


    void Start()
    {
        GenerateMap();

    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(DoAllMoves());//TODO: na pozniej
        }
    }
    IEnumerator DoAllMoves()
    {
        if (units != null)
        {
            foreach (Unit u in units)
            {
                yield return DoUnitMoves(u);
            }
        }
    }
    public IEnumerator DoUnitMoves(Unit u)
    {
        while (u.DoTurn())
        {
            while (animationisplaying)
            {
                yield return null;
            }
        }
    }

    public delegate void CitycreatedDelegate(City city, GameObject cityGO);
    public event CitycreatedDelegate OnCityCreated;
    public bool animationisplaying=true;
    public GameObject HexPrefab;
    public GameObject ForestPrefab;
    public GameObject RainForestPrefab;
    public Mesh WaterMesh;
    public Mesh FlatlandMesh;
    public Mesh HillMesh;
    public Mesh MountainMesh;

    public Material Ocean;
    public Material Plains;
    public Material Grass;
    public Material Mountain;
    public Material Desert;
    public GameObject UnitPrefab;
    public GameObject CityPrefab;

    [System.NonSerialized]public float MountainHeight = 1.0f;
    [System.NonSerialized] public float HillHeight = 0.6f;
    [System.NonSerialized] public float FlatHeight = 0.0f;

    [System.NonSerialized] public float MoistureForest = 0.5f;
    [System.NonSerialized] public float MoistureRainforest = 1f;
    [System.NonSerialized] public float MoistureGrass = 0f;
    [System.NonSerialized] public float MoisturePlains = -0.5f;

    //dodac mozliwosc zmiany przy tworzeniu mapy
    public readonly int Rows = 30;
    public readonly int Columns = 60;
    private Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;
    private Dictionary<GameObject, Hex> gameObjectToHexMap;

    private HashSet<Unit> units;
    private Dictionary<Unit, GameObject> unitToGameObjectMap;
    public Unit[] Units
    {
        get { return units.ToArray(); }
    }

    private HashSet<City> cities;
    private Dictionary<City, GameObject> cityToGameObjectMap;
    public City[] Cities
    {
        get { return cities.ToArray(); }
    }

    [System.NonSerialized] public bool allowWrapEastWest = true;  //czy mo¿na zapêtlaæ mapê lewo/prawo
    [System.NonSerialized] public bool allowWrapNorthSouth = false; //czy mo¿na zapêtlaæ mapê góra/dó³, nie uzywac
    public Hex GetHex(int x, int y)
    {
        if (hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated.");
            return null;
        }

        if (allowWrapEastWest)
        {
            x = x % Columns;
            if (x < 0)
            {
                x += Columns;
            }
        }
        if (allowWrapNorthSouth)
        {
            y = y % Rows;
            if (y < 0)
            {
                y += Rows;
            }
        }

        try
        {
            return hexes[x, y];
        }
        catch
        {
            Debug.LogError("GetHexAt: " + x + "," + y);
            return null;
        }
    }
    public Hex GetHexFromGo(GameObject hexGO)
    {
        if (gameObjectToHexMap.ContainsKey(hexGO))
        {
            return gameObjectToHexMap[hexGO];
        }
        return null;
    }
    public GameObject GetHexGo(Hex h)
    {
        if (hexToGameObjectMap.ContainsKey(h))
        {
            return hexToGameObjectMap[h];
        }
        return null;
    }
    public Vector3 GetHexPosition(int q, int r)
    {
        Hex hex = GetHex(q, r);
        return GetHexPosition(hex);
    }
    public Vector3 GetHexPosition(Hex hex)
    {
        return hex.PositionFromCamera(Camera.main.transform.position,Rows,Columns);
    }


    virtual public void GenerateMap()
    {
        hexes = new Hex[Columns, Rows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();
        gameObjectToHexMap = new Dictionary<GameObject, Hex>();

        //Generacja mapy z oceanem 
        for (int column = 0; column < Columns; column++)
        {
            for (int row = 0; row < Rows; row++)
            {

                Hex h = new Hex(this, column, row);
                h.Elevation = -1f;
                hexes[column, row] = h;
                Vector3 pos = h.PositionFromCamera(Camera.main.transform.position, Rows, Columns);
                //tworzenie instancji heksa
                GameObject hexGo = (GameObject)Instantiate(HexPrefab,pos,Quaternion.identity,this.transform);
                hexToGameObjectMap[h] = hexGo;
                gameObjectToHexMap[hexGo] = h;
                h.Elevationtype = Hex.Elevation_type.water;
                h.Terraintype = Hex.Terrain_Type.Ocean;
                hexGo.name = string.Format("{0},{1}", column, row);
                hexGo.GetComponent<HexComponent>().Hex = h;
                hexGo.GetComponent<HexComponent>().HexMap = this;
                hexGo.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}", column, row);

            }
        }
        updateHexes();
        
    }
    public void updateHexes()
    {

        for (int column = 0; column < Columns; column++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Hex h = hexes[column, row];
                GameObject hexGo = hexToGameObjectMap[h];
                MeshRenderer MRenderer = hexGo.GetComponentInChildren<MeshRenderer>();
                MeshFilter MFilter = hexGo.GetComponentInChildren<MeshFilter>();

                if (h.Elevation >= MountainHeight)
                {
                    h.Elevationtype = Hex.Elevation_type.mountain;
                    MRenderer.material = Mountain;
                    MFilter.mesh = MountainMesh;

                }
                else if (h.Elevation >= HillHeight)
                {
                    h.Elevationtype = Hex.Elevation_type.hill;
                    MRenderer.material = Grass;
                    MFilter.mesh = HillMesh;
                }
                else if (h.Elevation >= FlatHeight)
                {
                    h.Elevationtype = Hex.Elevation_type.Flat;
                    MRenderer.material = Plains;
                    MFilter.mesh = FlatlandMesh;
                }
                else
                {
                    h.Elevationtype = Hex.Elevation_type.water;
                    h.Terraintype = Hex.Terrain_Type.Ocean;
                    MRenderer.material = Ocean;
                    MFilter.mesh = WaterMesh;
                }
                if (h.Elevation >= FlatHeight && h.Elevation < MountainHeight)
                {
                    if (h.Moisture >= MoistureRainforest)
                    {
                        MRenderer.material = Grass;
                        h.Terraintype = Hex.Terrain_Type.Grasslands;
                        h.Featuretype = Hex.Feature_type.rainforest;
                        GameObject.Instantiate(RainForestPrefab, hexGo.transform.position, Quaternion.Euler(new Vector3(-90, 0, 0)), hexGo.transform);


                    }
                    else if (h.Moisture >= MoistureForest)
                    {
                        h.Terraintype = Hex.Terrain_Type.Grasslands;
                        h.Featuretype = Hex.Feature_type.forest;
                        MRenderer.material = Grass;
                        GameObject.Instantiate(ForestPrefab, hexGo.transform.position,Quaternion.Euler(new Vector3(-90,0,0)), hexGo.transform);

                    }
                    else if (h.Moisture >= MoistureGrass)
                    {
                        h.Terraintype = Hex.Terrain_Type.Grasslands;
                        h.Featuretype=Hex.Feature_type.none;
                        MRenderer.material = Grass;

                    }
                    else if(h.Moisture >= MoisturePlains)
                    {
                        h.Terraintype = Hex.Terrain_Type.Plains;
                        h.Featuretype = Hex.Feature_type.none;
                        MRenderer.material = Plains;

                    }
                    else
                    {
                        h.Terraintype = Hex.Terrain_Type.Desert;
                        h.Featuretype = Hex.Feature_type.none;
                        MRenderer.material = Desert;
                    }
                }

            }
        }
    }
    public Hex[] GetHexesInRadius(Hex centerTile, int radius)
    {
        List<Hex> results = new List<Hex>();

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
            {
                results.Add(GetHex(centerTile.Q + dx, centerTile.R + dy));
            }
        }
        return results.ToArray();

    }

    public void spawnUnitAt(Unit unit,GameObject prefab, int q, int r)
    {

        if (units == null)
        {
            units = new HashSet<Unit>();
            unitToGameObjectMap = new Dictionary<Unit, GameObject>();
        }
        Hex myHex = GetHex(q, r);
        GameObject myHexGO = hexToGameObjectMap[GetHex(q, r)];
        unit.SetHex(GetHex(q, r));
        GameObject unitGO = Instantiate(prefab, myHexGO.transform.position, Quaternion.identity, myHexGO.transform);
        unit.OnObjectMoved += unitGO.GetComponent<UnitView>().OnUnitMoved;
        units.Add(unit);
        unitToGameObjectMap.Add(unit, unitGO);
    }
    public void SpawnCityAt(City city, GameObject prefab, int q, int r)
    {
        if(cities == null)
        {
            cities = new HashSet<City>();
            cityToGameObjectMap = new Dictionary<City, GameObject>();
        }
        Hex myHex = GetHex(q, r);
        GameObject myHexGO = hexToGameObjectMap[GetHex(q, r)];

        city.SetHex(GetHex(q, r));
        GameObject cityGO = (GameObject)Instantiate(prefab, myHexGO.transform.position, Quaternion.identity, myHexGO.transform);

        cities.Add(city);
        cityToGameObjectMap.Add(city, cityGO);
        if(OnCityCreated != null)
        {
            OnCityCreated(city, cityGO);
        }
    }
}