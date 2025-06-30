using System;
using UnityEngine;

namespace TerritoryWars.DataModels.WebSocketEvents
{
    [Serializable]
    public struct MoveSneakPeek
    {
        public string Address;
        public bool IsJokerMode;
        public string Type;
        public Vector2Int Position;
        public int Rotation;
        public bool IsPlaced;
    }
}