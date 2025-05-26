using System;
using UnityEngine;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct Tile
    {
        public string Type;
        public Vector2Int Position;
        public byte Rotation;
        public byte PlayerSide;
    }
}