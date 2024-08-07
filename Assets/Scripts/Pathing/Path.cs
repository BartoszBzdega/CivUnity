using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    public class Path
    {

        public static T[] FindPath<T>(WorldPath world, PathableUnit unit, T StartTile, T EndTile, CostEstimateDelegate costEstimateFunc=null) where T:Tile
        {
            if (world == null || StartTile == null || EndTile == null || unit == null)
            {
                Debug.LogError(" find path zwraca nulle");
                return null;
            }

            Astar<T> resolver = new Astar<T>(world, unit, StartTile, EndTile, costEstimateFunc);
            resolver.DoWork();
            return resolver.GetList();
        }



        public delegate float CostEstimateDelegate(Tile a, Tile b);




    }
}