using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.DataModels.ClientEvents
{
    [Serializable]
    public struct TileSelected
    {
        public Vector2Int Position;
        public TileSelected(Vector2Int position)
        {
            Position = position;
        }
        
        public TileSelected(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }
    }
}