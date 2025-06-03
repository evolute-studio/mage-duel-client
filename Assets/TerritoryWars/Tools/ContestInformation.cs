using System;
using TerritoryWars.DataModels;
using UnityEngine;

namespace TerritoryWars.Tools
{
    public class ContestInformation
    {
        public Vector2Int Position;
        public Side Side;
        public StructureType StructureType;
        public Action ContestAction;

        public ContestInformation(Vector2Int position, Side side, StructureType structureType, Action contestAction)
        {
            Position = position;
            Side = side;
            ContestAction = contestAction;
            StructureType = structureType;
        }
    }

    // public enum ContestType
    // {
    //     Road,
    //     City,
    //     None
    // }
}