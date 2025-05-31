using System.Collections.Generic;
using System.Linq;
using TerritoryWars.DataModels;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars.Tile
{

    [System.Serializable]
    public class TileData
    {
        public string RotatedConfig => GetRotatedConfig();
        public string Type { get; private set; } = "FFFF";
        public Vector2Int Position { get; private set; } = new Vector2Int(-1, -1);
        public int Rotation { get; private set; } = 0;
        public int PlayerSide { get; private set; } = -1;
        
        //public string id;
        //public char[] sides;
        //public int rotationIndex = 0;
        //public int OwnerId = -1;
        
        public List<Sprite> HouseSprites = new List<Sprite>();

        public TileData()
        {
            Type = "FFFF"; // Default tile type, can be changed later
            Position = new Vector2Int(-1, -1); // Default position, can be set later
            Rotation = 0; // Default rotation
            PlayerSide = -1; // No owner by default
        }
        public TileData(TileModel tile)
        {
            Type = tile.Type;
            Position = tile.Position;
            Rotation = tile.Rotation;
            PlayerSide = tile.PlayerSide;
        }

        public TileData(string rotatedConfig, Vector2Int position, int playerSide)
        {
            UpdateData(rotatedConfig, position, playerSide);
        }

        public void UpdateData(string rotatedConfig, Vector2Int position = default, int playerSide = -2)
        {
            (byte type, byte rotation) = OnChainBoardDataConverter.GetTypeAndRotation(rotatedConfig);
            Type = GameConfiguration.GetTileType(type);
            Rotation = rotation;
            Position = position != default ? position : Position;
            PlayerSide = playerSide != -2 ? playerSide : PlayerSide;
        }
        
        public void SetOwner(int playerSide)
        {
            PlayerSide = playerSide;
        }

        public void Rotate(int times = 1)
        {
            Rotation = (Rotation + times) % 4;
        }

        public LandscapeType GetSide(Side side)
        {
            return CharToLandscape(RotatedConfig[(int)side]);
        }
        
        
        public List<Side> GetRoadSides()
        {
            List<Side> roadSides = new List<Side>();
            for (int i = 0; i < 4; i++)
            {
                if (RotatedConfig[i] == 'R')
                {
                    roadSides.Add((Side)i);
                }
            }
            return roadSides;
        }

        // public void SetConfig(string config)
        // {
        //     id = config;
        //     byte type;
        //     (type, rotationIndex) = OnChainBoardDataConverter.GetTypeAndRotation(config);
        //     sides = OnChainBoardDataConverter.TileTypes[type].ToCharArray();
        // }
        
        public bool IsCityParallel()
        {
            int cityCount = RotatedConfig.Count(c => c == 'C');
            if (cityCount != 2) return false;
            if ( (RotatedConfig[0] == 'C' && RotatedConfig[2] == 'C') || (RotatedConfig[1] == 'C' && RotatedConfig[3] == 'C')) return true;
            return false;
        }

        public string GetRotatedConfig()
        {
            char[] sides = Type.ToCharArray();
            char[] rotatedSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                int sourceIndex = (i - Rotation + 4) % 4;
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
            return Type.Contains('C');
        }

        public bool IsRoad()
        {
            return Type.Contains('R');
        }
    }
}
