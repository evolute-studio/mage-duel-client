using System;
using UnityEngine;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct Contested
    {
        public StructureType Type;
        public string BoardId;
        public Vector2Int Position;
        public Side Side;
        public byte WinnerId;
        public ushort BluePoints;
        public ushort RedPoints;
        
        public Contested SetData(evolute_duel_CityContestWon modelInstance)
        {
            Type = StructureType.City;
            BoardId = modelInstance.board_id.Hex();
            (Position, Side) = GameConfiguration.GetPositionAndSide(modelInstance.root);
            WinnerId = (byte)modelInstance.winner.Unwrap();
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
        
        public Contested SetData(evolute_duel_RoadContestWon modelInstance)
        {
            Type = StructureType.Road;
            BoardId = modelInstance.board_id.Hex();
            (Position, Side) = GameConfiguration.GetPositionAndSide(modelInstance.root);
            WinnerId = (byte)modelInstance.winner.Unwrap();
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
        
        public Contested SetData(evolute_duel_CityContestDraw modelInstance)
        {
            Type = StructureType.City;
            BoardId = modelInstance.board_id.Hex();
            (Position, Side) = GameConfiguration.GetPositionAndSide(modelInstance.root);
            WinnerId = 3; // No winner in draw
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
        
        public Contested SetData(evolute_duel_RoadContestDraw modelInstance)
        {
            Type = StructureType.Road;
            BoardId = modelInstance.board_id.Hex();
            (Position, Side) = GameConfiguration.GetPositionAndSide(modelInstance.root);
            WinnerId = 3; // No winner in draw
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
    }
}