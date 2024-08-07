using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class MapObject
{
    public MapObject()
    {

    }
    public int hitpoints = 100;
    public string name = "Test";
    public bool CanBeAttacked = true;
    public int faction = 0;
    public Hex Hex { get; protected set; }

    public delegate void ObjectMovedDelegae(Hex oldHex, Hex newHex);
    public ObjectMovedDelegae OnObjectMoved;
    virtual public void SetHex(Hex newHex)
    {
        Hex oldHex = Hex;


        Hex = newHex;


        if (OnObjectMoved != null)
        {
            OnObjectMoved(oldHex, newHex);
        }
    }
}
