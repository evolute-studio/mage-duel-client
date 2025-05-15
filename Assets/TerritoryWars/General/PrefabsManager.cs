using System;
using System.Collections.Generic;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace TerritoryWars.General
{
    public class PrefabsManager : MonoBehaviour
    {
        public static PrefabsManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogError("SessionManager already exists. Deleting new instance.");
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
            }
        }
        
        public TileAssetsObject TileAssetsObject;
        public GameObject[] Players;
        public GameObject MillPrefab;
        public GameObject ClashAnimationPrefab;
        public GameObject PinPrefab;
        public List<MineEnviromentTile> MineEnviromentTiles;
        public GameObject WallSegmentPrefab;
        private int _currentPlayerIndex = 0;
        public GameObject SkipBubblePrefab;
        public GameObject StructureHoverPrefab;
        public GameObject CloudPrefab;
        public GameObject[] MountainsGO;
        public GameObject[] SnowMountainsGO;
        [Header("Houses")]
        public ContestedHousesGameObject FirstPlayerContestedHouses;
        public ContestedHousesGameObject SecondPlayerContestedHouses;
        
        public NonContestedHousesGameObject FirstPlayerNonContestedHouses;
        public NonContestedHousesGameObject SecondPlayerNonContestedHouses;
        public NonContestedHousesGameObject NeutralPlayerNonContestedHouses;
        
        public GameObject GetNextPlayer()
        {
            _currentPlayerIndex = (_currentPlayerIndex + 1) % Players.Length;
            return Players[_currentPlayerIndex];
        }
        
        public GameObject GetPlayer(int index)
        {
            return Players[index];
        }
        
        public GameObject InstantiateObject(GameObject prefab)
        {
            return Instantiate(prefab);
        }

        public GameObject GetRandomMountainTile()
        {
            return MountainsGO[Random.Range(0, MountainsGO.Length)];
        }
        
        public GameObject GetRandomSnowMountainTile()
        {
            return SnowMountainsGO[Random.Range(0, SnowMountainsGO.Length)];
        }

        public GameObject GetRandomNotContestedHouseGameObject(int count, int playerIndex)
        {
            int randomHouseIndex;
            if (playerIndex == -1 || playerIndex == 3)
            {
                randomHouseIndex = Random.Range(0, NeutralPlayerNonContestedHouses.SmallHouses.Length);
                return NeutralPlayerNonContestedHouses.SmallHouses[randomHouseIndex].HouseGO;
            }        
            
            playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            var notContestedHouses = playerIndex == 0
                ? FirstPlayerNonContestedHouses
                : SecondPlayerNonContestedHouses;
            switch (count)
            {
                case 1:
                    randomHouseIndex = Random.Range(0, notContestedHouses.SmallHouses.Length);
                    return notContestedHouses.SmallHouses[randomHouseIndex].HouseGO;
                case 2:
                    randomHouseIndex = Random.Range(0, notContestedHouses.MediumHouses.Length);
                    return notContestedHouses.MediumHouses[randomHouseIndex].HouseGO;
                default:
                    return null;
                    
            }
        }

        public GameObject GetNonContestedHousePrefabByReference(Sprite sprite, int playerId = -1)
        {
            playerId = SetLocalPlayerData.GetLocalIndex(playerId);
            foreach (var housePrefab in FirstPlayerNonContestedHouses.SmallHouses)
            {
                if (housePrefab.HouseSprite == sprite)
                {
                    int index = Array.IndexOf(FirstPlayerNonContestedHouses.SmallHouses, housePrefab);
                    return playerId == 0 || playerId == -1 ? housePrefab.HouseGO : SecondPlayerNonContestedHouses.SmallHouses[index].HouseGO;
                }
            }

            foreach (var housePrefab in SecondPlayerNonContestedHouses.SmallHouses)
            {
                if (housePrefab.HouseSprite == sprite)
                {
                    int index = Array.IndexOf(SecondPlayerNonContestedHouses.SmallHouses, housePrefab);
                    return playerId == 1 || playerId == -1 ? housePrefab.HouseGO : FirstPlayerNonContestedHouses.SmallHouses[index].HouseGO;
                }
            }

            return null;
        }

        [Serializable]
        public class HousePrefab
        {
            public GameObject HouseGO;
            public Sprite HouseSprite;
        }

        public GameObject GetContestedHouse(int count, int playerIndex, Sprite house = null)
        {
            if (playerIndex == 3)
            {
                if (house != null)
                {
                    foreach (var smallHouse in TileAssetsObject.FirstPlayerHouses.SmallHouses)
                    {
                        if (smallHouse == house) playerIndex = 0;
                    }
                    
                    foreach (var largeHouse in TileAssetsObject.FirstPlayerHouses.LargeHouses)
                    {
                        if (largeHouse == house) playerIndex = 0;
                    }
                    
                    foreach (var smallHouse in TileAssetsObject.SecondPlayerHouses.SmallHouses)
                    {
                        if (smallHouse == house) playerIndex = 1;
                    }
                    
                    foreach (var largeHouse in TileAssetsObject.SecondPlayerHouses.LargeHouses)
                    {
                        if (largeHouse == house) playerIndex = 1;
                    }
                }
            }
            else
            {
                playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
            }
            
            var contestedHouses = playerIndex == 0 ?
                FirstPlayerContestedHouses :
                SecondPlayerContestedHouses;

            int randomIndex;

            switch (count)
            {
                case 1:
                    randomIndex = Random.Range(0, contestedHouses.OneHouse.Length);
                    return contestedHouses.OneHouse[randomIndex];
                case 2:
                    randomIndex = Random.Range(0, contestedHouses.DoubleHouse.Length);
                    return contestedHouses.DoubleHouse[randomIndex];
                case 3:
                    randomIndex = Random.Range(0, contestedHouses.TripleHouse.Length);
                    return contestedHouses.TripleHouse[randomIndex];
                case 4:
                    randomIndex = Random.Range(0, contestedHouses.QuadrupleHouse.Length);
                    return contestedHouses.QuadrupleHouse[randomIndex];
                default:
                    throw new ArgumentOutOfRangeException("Invalid house count: " + count);
            }
        }
        
        [Serializable]
        public class ContestedHousesGameObject
        {
            public GameObject[] OneHouse;
            public GameObject[] DoubleHouse;
            public GameObject[] TripleHouse;
            public GameObject[] QuadrupleHouse;
        }
        
        [Serializable]
        public class NonContestedHousesGameObject
        {
            public HousePrefab[] SmallHouses;
            public HousePrefab[] MediumHouses;
        }
    }
}