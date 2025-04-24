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
        public List<HousesSprites> FirstPlayerHousesAnimated;
        public List<HousesSprites> SecondPlayerHousesAnimated;
        public List<HousesSprites> NeutralHousesAnimated;
        
        [Header("First stage houses")]
        public NotContestedHouses FirstPlayerHouses;
        public NotContestedHouses SecondPlayerHouses;
        
        [Header("Second stage houses")]
        public ContestedHouses FirstPlayerContestedHouses;
        public ContestedHouses SecondPlayerContestedHouses;
        
        
        public Sprite[] Mountains;
        public GameObject ForestPrefab;
        public List<Sprite> RoadsSprites;
        public List<Sprite> RoadsSpritesContested;

        public Sprite[] WoodenPillars;
        public Sprite[] StonePillars;
        public Sprite[] WoodenWallSprites;
        public Sprite[] StoneWallSprites;
        public Sprite WoodenArc;
        public Sprite StoneArc;
        
        public Sprite MudCityTextureSprite;
        public Sprite StoneCityTextureSprite;
        
        public Sprite[] ContestedBlueHouses;
        public Sprite[] ContestedRedHouses;

        // 0 - neutral, 1 - first player, 2 - second player
        // 3 - neutral two points, 4 - first player two points, 5 - second player two points
        public Sprite[] Pins; 

        public int CurrentIndex { get; private set; } = 0;
        public int CurrentHouseIndex { get; private set; } = 0;

        public Sprite[] GetNextHouse(int playerIndex, bool chooseHighHouse = false)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            if (playerIndex == -1)
            {
                Sprite[] neutralNextHouseSprites;
                if (chooseHighHouse)
                {
                    neutralNextHouseSprites = NeutralHousesAnimated[1].DefaultSprites;
                    return neutralNextHouseSprites;
                }
                CurrentHouseIndex = (CurrentHouseIndex + 1) % NeutralHousesAnimated.Count;
                neutralNextHouseSprites = NeutralHousesAnimated[CurrentHouseIndex].DefaultSprites;
                return neutralNextHouseSprites;
            }

            if (chooseHighHouse)
            {
                List<HousesSprites>[] highHouses = { FirstPlayerHousesAnimated, SecondPlayerHousesAnimated };
                Sprite[] highHouse = highHouses[playerIndex][1].DefaultSprites;
                return highHouse;
            }
            
            List<HousesSprites>[] Houses = { FirstPlayerHousesAnimated, SecondPlayerHousesAnimated };
            CurrentHouseIndex = (CurrentHouseIndex + 1) % Houses[playerIndex].Count;
            Sprite[] nextHouseSprites = Houses[playerIndex][CurrentHouseIndex].DefaultSprites;
            return nextHouseSprites;
        }
        
        public Sprite[] GetHouseByReference(Sprite[] sprites, int playerIndex)
        {
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            foreach (var house in FirstPlayerHousesAnimated)
            {
                if (house.DefaultSprites == sprites)
                {
                    if (playerIndex == 0)
                        return house.DefaultSprites;
                    else
                    {
                        HousesSprites housesSprites = SecondPlayerHousesAnimated[FirstPlayerHousesAnimated.IndexOf(house)];
                        return housesSprites.DefaultSprites;
                    }
                }
            }

            foreach (var house in SecondPlayerHousesAnimated)
            {
                if (house.DefaultSprites == sprites)
                {
                    if (playerIndex == 1)
                        return house.DefaultSprites;
                    else
                    {
                        HousesSprites housesSprites = FirstPlayerHousesAnimated[SecondPlayerHousesAnimated.IndexOf(house)];
                        return housesSprites.DefaultSprites;
                    }
                }
            }

            int i = 0;
            foreach (var house in NeutralHousesAnimated)
            {
                if (house.DefaultSprites == sprites)
                {
                    if (playerIndex == 0)
                    {
                        return FirstPlayerHousesAnimated[i].DefaultSprites;
                    }

                    if (playerIndex == 1)
                    {
                        return SecondPlayerHousesAnimated[i].DefaultSprites;
                    }
                }
                i++;
            }

            return null;
        }

        public Sprite GetNotContestedHouse(int count, int playerIndex)
        {
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
        
        public Sprite[] GetHouseByReference(Sprite[] sprites, bool isContested = false)
        {
            foreach (var house in FirstPlayerHousesAnimated)
            {
                if (house.DefaultSprites == sprites)
                {
                    return house.DefaultSprites;
                }
            }

            foreach (var house in SecondPlayerHousesAnimated)
            {
                if (house.DefaultSprites == sprites)
                {
                    return house.DefaultSprites;
                }
            }

            foreach (var house in NeutralHousesAnimated)
            {
                if (house.DefaultSprites == sprites)
                {
                    return house.DefaultSprites;
                }
            }

            return null;
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

        public void BackIndex(int times)
        {
            CurrentHouseIndex = (CurrentHouseIndex - times) % FirstPlayerHousesAnimated.Count;
            if (CurrentHouseIndex < 0)
                CurrentHouseIndex += FirstPlayerHousesAnimated.Count;
        }

        public Sprite GetRandomMountain()
        {
            int randomIndex = Random.Range(0, Mountains.Length);
            Sprite randomMountain = Mountains[randomIndex];
            return randomMountain;
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
    }
}