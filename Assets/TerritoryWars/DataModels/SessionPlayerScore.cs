using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct SessionPlayerScore
    {
        public ushort CityScore;
        public ushort RoadScore;
        
        public SessionPlayerScore(ushort cityScore = 0, ushort roadScore = 0)
        {
            CityScore = cityScore;
            RoadScore = roadScore;
        }

        public ushort TotalScore => (ushort)(CityScore + RoadScore); 
    }
}
