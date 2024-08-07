using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Pathing
{


    public interface PathableUnit
    {
        float CostToEnterHex(Tile SourceTile, Tile DestinationTile);
    }
}