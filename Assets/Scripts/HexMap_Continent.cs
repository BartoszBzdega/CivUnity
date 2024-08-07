using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Continent : HexMap
{


    override public void GenerateMap()
    {
        //Tworzenie wszystkich heksow jako wody
        base.GenerateMap();

        int numContinents = 3;
        int continentSpacing = 20;


        Random.InitState(0);
        for (int c = 0; c < numContinents; c++)
        {
            int numSplats = Random.Range(4, 8);
            for (int i = 0; i < numSplats; i++)
            {
                int range = Random.Range(3, 6);
                int y = Random.Range(range, Rows - range);
                int x = Random.Range(0, 10) - y / 2 + (c * continentSpacing);

                ElevateArea(x, y, range);
            }

        }

        float noiseResolution = 0.01f;
        Vector2 noiseOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        float noiseScale = 2.5f; // im wieksza wartosc tym wiecej wysp
        for (int column = 0; column < Columns; column++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Hex h = GetHex(column, row);
                float n =
                Mathf.PerlinNoise(((float)column / Mathf.Max(Columns, Rows) / noiseResolution) + noiseOffset.x,((float)row / Mathf.Max(Columns, Rows) / noiseResolution) + noiseOffset.y)- 0.5f;
                h.Elevation += n * noiseScale;
            }
        }

        noiseResolution = 0.01f;
        noiseOffset = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        noiseScale = 2.5f; // im wieksza wartosc tym wiecej wysp
        for (int column = 0; column < Columns; column++)
        {
            for (int row = 0; row < Rows; row++)
            {
                Hex h = GetHex(column, row);
                float n =
                Mathf.PerlinNoise(((float)column / Mathf.Max(Columns, Rows) / noiseResolution) + noiseOffset.x, ((float)row / Mathf.Max(Columns, Rows) / noiseResolution) + noiseOffset.y) - 0.5f;
                h.Moisture += n * noiseScale;
            }
        }
        updateHexes();
        Unit unit = new Unit();
        unit.canSettleCities = true;
        spawnUnitAt(unit,UnitPrefab, 1, 9);
    }

    void ElevateArea(int q, int r, int range, float centerHeight = .8f)
    {

        Hex centerHex = GetHex(q, r);


        Hex[] areaHexes = GetHexesInRadius(centerHex, range);
        foreach (Hex h in areaHexes)
        {
            if(h.Elevation < 0)
            {
                h.Elevation = 0;
            }
            h.Elevation = centerHeight * Mathf.Lerp(1f, 0.25f, Mathf.Pow(Hex.Distance(centerHex, h) / range, 2f)); //im dalej od œrodkowego heksa tym ni¿sza elewacja terenu
        }
    }
}
