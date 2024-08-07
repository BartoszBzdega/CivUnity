using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    public interface Tile
    {
        public Tile[] GetNeighbours();
        float AggregateCostToEnter(float cost, Tile SourceTilce, PathableUnit unit);


    }
}