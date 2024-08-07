using JetBrains.Annotations;
using Pathing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
public class Hex:Tile
{

    public Hex(HexMap hexMap, int q, int r)
    {
        this.hexMap = hexMap;
        this.Q = q;
        this.R = r;
        this.S = -(q + r);
        units = new HashSet<Unit>();
    }
    HashSet<Unit> units;
    public Unit[] Units
    {
        get {return units.ToArray();} 
    }


    public City city { get; set; }
    public readonly HexMap hexMap;
    public readonly int Q;  //Kolumna
    public readonly int R;  // Rzad
    public readonly int S;


    
    public float Elevation;
    public float Moisture;

    public enum Terrain_Type { Plains, Grasslands, Desert, Ocean}
    public enum Elevation_type { Flat, hill, mountain, water}

    public Terrain_Type Terraintype { get; set; }
    public Elevation_type Elevationtype { get; set; }
    public enum Feature_type { none, forest, rainforest, swamp }
    public Feature_type Featuretype { get; set; }




    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    float radius = 1f;
    bool allowWrapEastWest = true;  //czy mo¿na zapêtlaæ mapê lewo/prawo
    bool allowWrapNorthSouth = false; //czy mo¿na zapêtlaæ mapê góra/dó³


    public Vector3 Position()
    {
        return new Vector3(HexHorizontalSpacing() * (this.Q + this.R / 2f),0,HexVerticalSpacing() * this.R);

    }

    public float HexHeight() //maksymalna wysokosc heksa
    {
        return radius * 2;
    }

    public float HexWidth() //maksymalna szerokosc heksa
    {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    public float HexVerticalSpacing()
    {
        return HexHeight() * 0.75f;
    }


    public float HexHorizontalSpacing()
    {
        return HexWidth();
    }
    public Vector3 PositionFromCamera()
    {
        return hexMap.GetHexPosition(this);
    }

    public Vector3 PositionFromCamera(Vector3 cameraPosition, float numRows, float numColumns)
    {
        float mapHeight = numRows * HexVerticalSpacing();
        float mapWidth = numColumns * HexHorizontalSpacing();

        Vector3 position = Position();

        if (allowWrapEastWest)
        {
            float WidthsFromCamera = (position.x - cameraPosition.x) / mapWidth;

           
            if (WidthsFromCamera > 0)
                WidthsFromCamera += 0.5f;
            else
                WidthsFromCamera -= 0.5f;

            int WidthToFix = (int)WidthsFromCamera;

            position.x -= WidthToFix * mapWidth;

        }

        if (allowWrapNorthSouth)
        {
            float howManyHeightsFromCamera = (position.z - cameraPosition.z) / mapHeight;

           
            if (howManyHeightsFromCamera > 0)
                howManyHeightsFromCamera += 0.5f;
            else
                howManyHeightsFromCamera -= 0.5f;

            int HeightsToFix = (int)howManyHeightsFromCamera;

            position.z -= HeightsToFix * mapHeight;
        }
        return position;
    }
    public static float CostEstimate(Tile aa, Tile bb)
    {
        return Distance((Hex)aa, (Hex)bb);
    }

    public static float Distance(Hex a, Hex b)
    {
        int DQ = Mathf.Abs(a.Q - b.Q);
        if (a.hexMap.allowWrapEastWest)
        {
            if (DQ > a.hexMap.Columns / 2)
            {
                DQ = a.hexMap.Columns - DQ;
            }
        }
        int DR = Mathf.Abs(a.Q - b.Q);
        if (a.hexMap.allowWrapNorthSouth)
        {
            if (DR > a.hexMap.Rows / 2)
            {
                DR = a.hexMap.Rows - DR;
            }
        }
        return Mathf.Max(Mathf.Abs(a.Q - b.Q), Mathf.Abs(a.R - b.R), Mathf.Abs(a.S - b.S));
    }

    public static float DistanceWrap(Hex a, Hex b, int NumColumns)
    {
        int distance = Mathf.Abs(a.Q - b.Q);
        if (distance >NumColumns/2)
        {
            distance=NumColumns-distance;
        }
        return Mathf.Max(distance, Mathf.Abs(a.R - b.R), Mathf.Abs(a.S - b.S));
    }
    public void AddUnit (Unit unit)
    {
        if(units == null)
        {
            units = new HashSet<Unit>();
        }
        units.Add(unit);
     
    }
    public void AddCity(City city)
    {
        if(city!=null)
        {
            return;
        }
        this.city = city;
    }
    public void RemoveCity(City city)
    {
        if (city == null)
        {
            return;
        }
        if (this.city != city)
        {
            return;
        }
        this.city = null;
    }
    public void RemoveUnit(Unit unit)
    {
        if(units!= null)
        {
            units.Remove(unit);
        }
    }
 

    public int BaseMovementCost(bool isFlier, bool isFasterinHills, bool isFaterinForest)
    {
        int moveCost=1;
        if (Elevationtype == Elevation_type.hill && isFasterinHills == false)
        {
            moveCost += 1;
        }
        if ((Elevationtype == Elevation_type.mountain || Elevationtype == Elevation_type.water) && isFlier==false)
        {
            return -99;
        }
        if ((Featuretype == Feature_type.forest || Featuretype==Feature_type.rainforest )&& isFaterinForest == false)
        {
            moveCost += 1;
        }
        return moveCost;

    }
    Hex[] neighbours;
    public Tile[] GetNeighbours()
    {
        if (this.neighbours != null)
        {
            return this.neighbours;
        }
        List<Hex> neighbours = new List<Hex>();
        neighbours.Add(hexMap.GetHex(Q+1, R +0));
        neighbours.Add(hexMap.GetHex(Q+-1,R + 0));
        neighbours.Add(hexMap.GetHex(Q + 0,R + +1));
        neighbours.Add(hexMap.GetHex(Q + 0,R + -1));
        neighbours.Add(hexMap.GetHex(Q + +1,R + -1));
        neighbours.Add(hexMap.GetHex(Q + -1,R + +1));
        List<Hex> neighbours2 = new List<Hex>();
        foreach(Hex n in neighbours)
        {
            if (n != null)
            {
                neighbours2.Add(n);
            }
        }

        this.neighbours= neighbours2.ToArray();
        return this.neighbours;
    }

    public float AggregateCostToEnter(float cost, Tile SourceTilce, PathableUnit unit)
    {

        return ((Unit)unit).AgreggateTurnsToEnterHex(this, cost);
    }
}