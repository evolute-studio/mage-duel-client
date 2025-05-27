using System;
using UnityEngine;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct TileModel
    {
        public string Type;
        public Vector2Int Position;
        public int Rotation;
        public int PlayerSide; // -1 for neutral, 0 for blue, 1 for red
        
        public TileModel(string type = "FFFF", int rotation = 0, Vector2Int position = default, int playerSide = -1)
        {
            Type = type;
            Position = position;
            Rotation = rotation;
            PlayerSide = playerSide;
        }
    }
}