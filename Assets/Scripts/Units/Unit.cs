using Pathing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Unit : MapObject, PathableUnit
{
    public Unit()
    {
        name = "Unit";
    }




    public int strength = 8;
    public int movement = 2;
    public int movementRemaining = 2;
    public bool canSettleCities = false;




    List<Hex> HexPath;
    bool MOVEMENT_RULES_TEST = false;


    public override void SetHex(Hex newHex)
    {
        if (Hex != null)
        {
            Hex.RemoveUnit(this);
        }
        base.SetHex(newHex);
        Hex.AddUnit(this);

    }
    public void TEST_PATHING_FUNCTION()
    {
        //Pathing.Path.CostEstimateDelegate ced = (Tile a, Tile b) => (Hex.Distance(a, b));
        Hex[] pathHexes = Pathing.Path.FindPath<Hex>(Hex.hexMap,this,Hex,Hex.hexMap.GetHex(Hex.Q+2,Hex.R),Hex.CostEstimate);
        setHexPath(pathHexes);
    }
    public Hex[] GetHexPath()
    {
        return (this.HexPath==null)? null:this.HexPath.ToArray();
    }

    public void ClearHexPath()
    {
        this.HexPath = new List<Hex>();
    }

    public void setHexPath(Hex[] hexPath)
    {
        this.HexPath = new List<Hex>(hexPath);

    }
    public bool UnitWaitingForOrders()
    {
        if (movementRemaining > 0 && (HexPath == null || HexPath.Count == 0))
        {
            return true;
        }
    return false;
    }

    public bool DoTurn()
    {
        Debug.Log("Tura");
        if(movementRemaining <= 0) 
        {
            return false;
        }
        if (HexPath == null || HexPath.Count == 0)
        {
            return false;
        }
        Hex HexLeaving = HexPath[0];
        Hex newHex=HexPath[1];

        int costToEnter = MovementCostToEnterHex(newHex);
        if(costToEnter > movementRemaining&& MOVEMENT_RULES_TEST&&movementRemaining<movement) 
        {
            HexPath.Insert(0, HexLeaving);
            return false;
        }
        HexPath.RemoveAt(0);

        if (HexPath.Count == 1)
        {
            HexPath = null;
        }

        SetHex(newHex);
        movementRemaining = Mathf.Max(movementRemaining-costToEnter, 0);
        return HexPath!=null&&movementRemaining>0;
    }
    public int MovementCostToEnterHex(Hex hex)
    {

        return hex.BaseMovementCost(false, false, false);
    }
    public float AgreggateTurnsToEnterHex(Hex hex, float turnsToDate)
    {
        float baseTurnsToEnterHex = MovementCostToEnterHex(hex)/movement;

        if(baseTurnsToEnterHex < 0)
        {
            return -999999;
        }

        if(baseTurnsToEnterHex>1)
        {
            baseTurnsToEnterHex = 1;
        }

        float turnsRemaining = movementRemaining / movement;
        float turnsToDateWhole = Mathf.Floor(turnsToDate);
        float turnsToDateFraction = turnsToDate - turnsToDateWhole;
        if(turnsToDateFraction > 0 && turnsToDateFraction <0.01f || turnsToDateFraction > 0.99f)
        {
            if(turnsToDateFraction < 0.01f)
            {
                turnsToDateFraction = 0;
            }
            if (turnsToDateFraction > 0.99f)
            {
                turnsToDateWhole += 1;
                turnsToDateFraction = 0;
            }
        }
        float turnsUsedAfterThismove = turnsToDateFraction + baseTurnsToEnterHex;
        if(turnsUsedAfterThismove>1)
        {
            if (MOVEMENT_RULES_TEST)
            {
                if (turnsToDateFraction == 0)
                {

                }
                else
                {
                    turnsToDateWhole += 1;
                    turnsToDateFraction = 0;
                }
                turnsUsedAfterThismove = baseTurnsToEnterHex;
            }
            else
            {
                turnsUsedAfterThismove = 1;
            }
        }
        return turnsToDateWhole+turnsUsedAfterThismove;
    }
    public void ResetMovement()
    {
        movementRemaining = movement;
    }
    public float CostToEnterHex(Tile SourceTile, Tile DestinationTile)
    {
        return 1;
    }
}
