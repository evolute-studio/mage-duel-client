using System;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct Contested
    {
        public ContestedType Type;
        public string BoardId;
        public byte Root;
        public byte WinnerId;
        public ushort BluePoints;
        public ushort RedPoints;
        
        public Contested SetData(evolute_duel_CityContestWon modelInstance)
        {
            Type = ContestedType.City;
            BoardId = modelInstance.board_id.Hex();
            Root = modelInstance.root;
            WinnerId = (byte)modelInstance.winner.Unwrap();
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
        
        public Contested SetData(evolute_duel_RoadContestWon modelInstance)
        {
            Type = ContestedType.Road;
            BoardId = modelInstance.board_id.Hex();
            Root = modelInstance.root;
            WinnerId = (byte)modelInstance.winner.Unwrap();
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
        
        public Contested SetData(evolute_duel_CityContestDraw modelInstance)
        {
            Type = ContestedType.City;
            BoardId = modelInstance.board_id.Hex();
            Root = modelInstance.root;
            WinnerId = 3; // No winner in draw
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
        
        public Contested SetData(evolute_duel_RoadContestDraw modelInstance)
        {
            Type = ContestedType.Road;
            BoardId = modelInstance.board_id.Hex();
            Root = modelInstance.root;
            WinnerId = 3; // No winner in draw
            BluePoints = modelInstance.blue_points;
            RedPoints = modelInstance.red_points;
            return this;
        }
    }
    
    public enum ContestedType
    {
        Road,
        City,
    }
}