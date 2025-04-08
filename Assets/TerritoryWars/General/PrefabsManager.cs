using System.Collections.Generic;
using TerritoryWars.ScriptablesObjects;
using UnityEngine;

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
    }
}