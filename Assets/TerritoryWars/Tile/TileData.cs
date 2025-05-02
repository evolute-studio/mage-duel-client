using System.Collections.Generic;
using System.Linq;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.Tile
{

    [System.Serializable]
    public class TileData
    {
        public Structure CityStructure;
        public Structure RoadStructure;
        public string id;
        private char[] sides;
        public int rotationIndex = 0;
        public int OwnerId = -1;
        
        public List<Sprite> HouseSprites = new List<Sprite>();
        

        public TileData(string tileCode)
        {
            sides = tileCode.ToCharArray();
            UpdateId();
        }

        private void UpdateId()
        {
            
            char[] rotatedSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                
                // Top(0) -> Right(1)
                // Right(1) -> Bottom(2)
                // Bottom(2) -> Left(3)
                // Left(3) -> Top(0)
                int sourceIndex = (i - rotationIndex + 4) % 4;
                rotatedSides[i] = sides[sourceIndex];
            }
            id = new string(rotatedSides);
        }

        public void Rotate(int times = 1)
        {
           
            rotationIndex = (rotationIndex + times) % 4;
            UpdateId();
        }

        public LandscapeType GetSide(Side side)
        {
          
            int index = ((int)side - rotationIndex + 4) % 4;
            return CharToLandscape(sides[index]);
        }
        
        public void SetCityStructure(Structure structure)
        {
            this.CityStructure = structure;
            CityStructure.OwnerId = structure.OwnerId;
        }
        
        public void SetRoadStructure(Structure structure)
        {
            this.RoadStructure = structure;
        }
        
        public void SetCityOwner(int playerId)
        {
            OwnerId = playerId;
            
        }

        public string GetConfig()
        {
            return id; //+ ":" + rotationIndex;
        }
        
        public List<Side> GetRoadSides()
        {
            List<Side> roadSides = new List<Side>();
            for (int i = 0; i < 4; i++)
            {
                if (sides[i] == 'R')
                {
                    roadSides.Add((Side)((i + rotationIndex) % 4));
                }
            }
            return roadSides;
        }
        
        public string GetConfigWithoutRotation()
        {
            return id;
        }

        public void SetConfig(string config)
        {
            id = config;
            (_, rotationIndex) = OnChainBoardDataConverter.GetTypeAndRotation(config);
        }
        
        public bool IsCityParallel()
        {
            int cityCount = id.Count(c => c == 'C');
            if (cityCount != 2) return false;
            if ( (id[0] == 'C' && id[2] == 'C') || (id[1] == 'C' && id[3] == 'C')) return true;
            return false;
        }

        public static string GetRotatedConfig(string config, int times = 1)
        {
            char[] sides = config.ToCharArray();
            char[] rotatedSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                int sourceIndex = (i - times + 4) % 4;
                rotatedSides[i] = sides[sourceIndex];
            }
            return new string(rotatedSides);
        }

        public static LandscapeType CharToLandscape(char c)
        {
            return c switch
            {
                'C' => LandscapeType.City,
                'R' => LandscapeType.Road,
                'F' => LandscapeType.Field,
                _ => throw new System.ArgumentException($"Invalid landscape type: {c}")
            };
        }

        public bool IsCity()
        {
            // check in Id if there is C
            return id.Contains('C');
        }

        public bool IsRoad()
        {
            return id.Contains('R');
        }
    }
}