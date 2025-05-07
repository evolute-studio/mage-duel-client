using System;
using System.Collections.Generic;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace TerritoryWars.ScriptablesObjects
{
    [CreateAssetMenu(fileName = "TileAssetsObject", menuName = "TileAssetsObject", order = 0)]
    public class TileAssetsObject : ScriptableObject
    {
        [Header("First stage houses")]
        public NotContestedHouses FirstPlayerHouses;
        public NotContestedHouses SecondPlayerHouses;
        public NotContestedHouses NeutralPlayerHouses;
        
        [Header("Second stage houses")]
        public ContestedHouses FirstPlayerContestedHouses;
        public ContestedHouses SecondPlayerContestedHouses;
        
        
        public Sprite[] ForestMountains;
        public Sprite[] SnowMountains;
        public Sprite[] ForestMountainsWithoutClouds;
        public GameObject ForestPrefab;
        public List<Sprite> RoadsSprites;
        public List<Sprite> RoadsSpritesContested;

        public Sprite[] WoodenPillars;
        public Sprite[] StonePillars;
        public Sprite[] WoodenWallSprites;
        public Sprite[] StoneWallSprites;

        public Sprite[] HangingGrass; 
        
        public Sprite MudCityTextureSprite;
        public Sprite StoneCityTextureSprite;
        
        public Sprite[] ContestedBlueHouses;
        public Sprite[] ContestedRedHouses;
        
        [Header("Trees")]
        public Sprite[] NorthernTrees;
        public Sprite[] SouthernTrees;

        [Header("Flags")] public FlagsOnWall[] FlagsOnWalls;

        // 0 - neutral, 1 - first player, 2 - second player
        // 3 - neutral two points, 4 - first player two points, 5 - second player two points
        public Sprite[] Pins; 

        public int CurrentIndex { get; private set; } = 0;
        public int CurrentHouseIndex { get; private set; } = 0;
        
        public Sprite GetNotContestedHouse(int count, int playerIndex)
        {
            if (playerIndex == -1)
            {
                int randomNeutralIndex = Random.Range(0,NeutralPlayerHouses.SmallHouses.Length);
                return NeutralPlayerHouses.SmallHouses[randomNeutralIndex];
            }
            
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            var notContestedHouses = playerIndex == 0 ? FirstPlayerHouses : SecondPlayerHouses;
            int randomIndex;
            switch (count)
            {
                case 1:
                    randomIndex = Random.Range(0, notContestedHouses.SmallHouses.Length);
                    return notContestedHouses.SmallHouses[randomIndex];
                case 2:
                    randomIndex = Random.Range(0, notContestedHouses.LargeHouses.Length);
                    return notContestedHouses.LargeHouses[randomIndex];
                default:
                    return null;
            }
        }
        
        public Sprite GetNotContestedHouseByReference(Sprite sprite, int playerIndex)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            Sprite[] houses = FirstPlayerHouses.SmallHouses;
            for (int i = 0; i < houses.Length; i++)
            {
                if (houses[i] == sprite)
                {
                    if (playerIndex == 0)
                        return houses[i];
                    return SecondPlayerHouses.SmallHouses[i];
                }
            }
            houses = FirstPlayerHouses.LargeHouses;
            for (int i = 0; i < houses.Length; i++)
            {
                if (houses[i] == sprite)
                {
                    if (playerIndex == 0)
                        return houses[i];
                    return SecondPlayerHouses.LargeHouses[i];
                }
            }
            houses = SecondPlayerHouses.SmallHouses;
            for (int i = 0; i < houses.Length; i++)
            {
                if (houses[i] == sprite)
                {
                    if (playerIndex == 1)
                        return houses[i];
                    return FirstPlayerHouses.SmallHouses[i];
                }
            }
            houses = SecondPlayerHouses.LargeHouses;
            for (int i = 0; i < houses.Length; i++)
            {
                if (houses[i] == sprite)
                {
                    if (playerIndex == 1)
                        return houses[i];
                    return FirstPlayerHouses.LargeHouses[i];
                }
            }
            return null;
        }

        public Sprite GetContestedHouses(int count, int playerIndex)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            var contestedHouses = playerIndex == 0 ? FirstPlayerContestedHouses : SecondPlayerContestedHouses;
            int randomIndex;
            switch (count)
            {
                case 1:
                    randomIndex = Random.Range(0, contestedHouses.OneHouse.Length);
                    return contestedHouses.OneHouse[randomIndex];
                case 2:
                    randomIndex = Random.Range(0, contestedHouses.DoubleHouses.Length);
                    return contestedHouses.DoubleHouses[randomIndex];
                case 3:
                    randomIndex = Random.Range(0, contestedHouses.TripleHouses.Length);
                    return contestedHouses.TripleHouses[randomIndex];
                case 4:
                    randomIndex = Random.Range(0, contestedHouses.QuadrupleHouses.Length);
                    return contestedHouses.QuadrupleHouses[randomIndex];
                default:
                    throw new ArgumentOutOfRangeException("Invalid house count" + count);
            }
        }

        public bool IsContestedHouse(Sprite sprite, int count, int playerIndex)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            var contestedHouses = playerIndex == 0 ? FirstPlayerContestedHouses : SecondPlayerContestedHouses;
            Sprite[] houses = null;
            switch (count)
            {
                case 1:
                    houses = contestedHouses.OneHouse;
                    break;
                case 2:
                    houses = contestedHouses.DoubleHouses;
                    break;
                case 3:
                    houses = contestedHouses.TripleHouses;
                    break;
                case 4:
                    houses = contestedHouses.QuadrupleHouses;
                    break;
            }

            if (houses != null)
                foreach (var house in houses)
                {
                    if (house == sprite) return true;
                }

            return false;
        }
        
        public Sprite GetTree(bool isNorth)
        {
            int randomIndex = Random.Range(0, (isNorth ? NorthernTrees : SouthernTrees).Length);
            Sprite randomTree = (isNorth ? NorthernTrees : SouthernTrees)[randomIndex];
            return randomTree;
        }
        
        public Sprite GetContestedRoadByReference(Sprite roadSprite)
        {
            for(int i = 0; i < RoadsSprites.Count; i++)
            {
                if (RoadsSprites[i] == roadSprite)
                {
                    return RoadsSpritesContested[i];
                }
            }
            return roadSprite;
        }

        public Sprite GetPillar(bool isContested)
        {
            int randomIndex = Random.Range(0, (isContested ? StonePillars : WoodenPillars).Length);
            Sprite randomPillar = (isContested ? StonePillars : WoodenPillars)[randomIndex];
            return randomPillar;
        }
        
        public Sprite GetWall(bool isContested)
        {
            int randomIndex = Random.Range(0, (isContested ? StoneWallSprites : WoodenWallSprites).Length);
            Sprite randomWall = (isContested ? StoneWallSprites : WoodenWallSprites)[randomIndex];
            return randomWall;
        }
        
        public Sprite GetPinByPlayerId(int playerId)
        {
            playerId = SetLocalPlayerData.GetLocalIndex(playerId);
            int id = playerId + 1;
            return Pins[id];
        }
        
        public Sprite GetCityGroundTexture(bool isContested)
        {
            return isContested ? StoneCityTextureSprite : MudCityTextureSprite;
        }
        
        public Sprite[] GetRandomContestedHouse(int playerIndex)
        {
            int randomIndex = Random.Range(0, (playerIndex == 0 ? ContestedBlueHouses : ContestedRedHouses).Length);
            Sprite randomContestedHouse = (playerIndex == 0 ? ContestedBlueHouses : ContestedRedHouses)[randomIndex];
            return new Sprite[] {randomContestedHouse};
        }
        
        public Sprite GetHangingGrass()
        {
            int randomIndex = Random.Range(0, HangingGrass.Length);
            Sprite randomHangingGrass = HangingGrass[randomIndex];
            return randomHangingGrass;
        }
        
        public Sprite GetFlagByReference(int winner, Sprite sprite)
        {
            for (int i = 0; i < FlagsOnWalls.Length; i++)
            {
                for(int j = 0; j < FlagsOnWalls[i].Flags.Length; j++)
                {
                    if (FlagsOnWalls[i].Flags[j] == sprite)
                    {
                        return FlagsOnWalls[i].Flags[winner];
                    }
                }
            }
            return null;
        }

    public Sprite GetRandomMountain(int boardPart)
    {
            int randomIndex;
            Sprite randomMountain;
        
            switch (boardPart)
            {
                case 0:
                case 1:
                case 2:
                    // without clouds
                    randomIndex = Random.Range(0, ForestMountainsWithoutClouds.Length);
                    randomMountain = ForestMountainsWithoutClouds[randomIndex];
                    return randomMountain;
                    break;
                case 3:
                    // snow
                    randomIndex = Random.Range(0, SnowMountains.Length);
                    randomMountain = SnowMountains[randomIndex];
                    return randomMountain;
                    break;
                case -1:
                    // without clouds
                    return null;
                    break;
            }
            return null;
        }

        [Serializable]
        public class HousesSprites
        {
            [FormerlySerializedAs("HousesSprites")] public Sprite[] DefaultSprites;
        }


        [Serializable]
        public class NotContestedHouses
        {
            public Sprite[] SmallHouses;
            public Sprite[] LargeHouses;
        }
        
        [Serializable]
        public class ContestedHouses
        {
            public Sprite[] OneHouse;
            public Sprite[] DoubleHouses;
            public Sprite[] TripleHouses;
            public Sprite[] QuadrupleHouses;
        }

        [Serializable]
        public class FlagsOnWall
        {
            public Sprite[] Flags;
        }
        
    }
}