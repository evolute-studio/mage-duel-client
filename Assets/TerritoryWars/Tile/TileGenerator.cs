using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NUnit.Framework;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.Tools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Sequence = DG.Tweening.Sequence;

namespace TerritoryWars.Tile
{
    public class TileGenerator : MonoBehaviour
    {
        public TileConnector[] connectors;
        public SpriteRenderer RoadRenderer;
        public TileAssetsObject TileAssetsObject => PrefabsManager.Instance.TileAssetsObject;
        [FormerlySerializedAs("TileRenderers")] public TileParts tileParts;
        public TileJokerAnimator TileJokerAnimator;

        public TileRotator TileRotator;
        public GameObject City { get; private set; }
        public GameObject Mill { get; private set; }

        private TileData _tileData;

        [Header("Tile Prefabs")] public List<TilePrefab> TilePrefabs;

        [Header("Roads")] public List<RoadPair> RoadPairs;

        [Header("Cities")] public List<CityData> CityData;

        [HideInInspector] public string TileConfig;

        private int _currentHouseIndex = 0;

        public List<SpriteRenderer> AllCityRenderers = new List<SpriteRenderer>();
        public List<LineRenderer> AllCityLineRenderers = new List<LineRenderer>();
        
        public List<SpriteRenderer> houseRenderers;
        public List<RoadPin> Pins = new List<RoadPin>();
        

        private byte _rotation;
        private bool _isTilePlacing;
        private Vector2Int _placingTilePosition;

        public GameObject CurrentTileGO { get; private set; }
        public WallPlacer WallPlacer { get; private set; }


        public void Awake() => Initialize();

        public void Initialize()
        {
            TileRotator.OnRotation.AddListener(Rotate);
        }

        public void SetConnectorTypes()
        {
            for (int i = 0; i < connectors.Length; i++)
            {
                connectors[i].SetLandscape(TileData.CharToLandscape(TileConfig[i]));
            }
        }

        public void Generate(TileData data, bool isTilePlacing = false, Vector2Int placingTilePosition = default)
        {
            TileConfig = data.id;
            _tileData = data;
            _isTilePlacing = isTilePlacing;
            _placingTilePosition = placingTilePosition;
            Generate();
        }

        public void Generate()
        {
            Destroy(CurrentTileGO);
            CurrentTileGO = null;
            
            foreach (var pin in Pins)
            {
                if(pin == null) continue;
                pin.transform.DOKill();
                Destroy(pin.gameObject);
            }
            Pins.Clear();
            AllCityRenderers = new List<SpriteRenderer>();
            AllCityLineRenderers = new List<LineRenderer>();

            TileRotator.ClearLists();

            var tileConfig = OnChainBoardDataConverter.GetTypeAndRotation(TileConfig);
            string id = OnChainBoardDataConverter.TileTypes[tileConfig.Item1];
            _rotation = (byte)((tileConfig.Item2 + 1) % 4);
            
            foreach (var tile in TilePrefabs)
            {
                if (tile.Config == id)
                {
                        CurrentTileGO = Instantiate(tile.TilePrefabGO, transform);
                    InitializeTile();
                    break;
                }
            }
        }

        public void Rotate()
        {
            string s = "Previous config: " + TileConfig;
            TileConfig = TileData.GetRotatedConfig(TileConfig);
            s += " Rotated config: " + TileConfig;
            Debug.Log(s);
            SetConnectorTypes();
            
            InitializeTile();
            
        }

        private void GenerateRoadPins(Transform[] points)
        {
            int playerId = SessionManager.Instance.CurrentTurnPlayer != null
                ? SessionManager.Instance.CurrentTurnPlayer.LocalId
                : -1;
            
            float randomStartDelay = Random.Range(0f, 2f);
            RoadPin[] pins = new RoadPin[4];
            char[] id = TileConfig.ToCharArray();
            for (int i = 0; i < id.Length; i++)
            {
                if (id[i] == 'R')
                {
                    GameObject pin = Instantiate(PrefabsManager.Instance.PinPrefab, points[i]);
                    RoadPin roadPin = pin.GetComponent<RoadPin>();
                    if (_tileData.OwnerId == -1) playerId = -1;
                    int score = GetRoadPoints(TileConfig);
                    roadPin.Initialize(playerId, score);
                    pin.transform.parent = points[i];
                    roadPin.transform.localPosition = Vector3.zero;
                    
                    Sequence sequence = DOTween.Sequence();
                    sequence.AppendInterval(randomStartDelay);
                    sequence.Append(roadPin.transform.DOLocalMoveY(0.035f, 2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad));
                    sequence.Play();
                    
                    pins[i] = roadPin;
                }
            }
            Pins.AddRange(pins);
            
        }

        private int GetRoadPoints(string config)
        {
            int roadCount = TileConfig.Count(c => c == 'R');
    
            bool isCRCR = config == "CRCR" || config == "RCRC";
            if (isCRCR) return 1;
            if (roadCount == 2) return 2;
            return 1;
        }
        
        public void InitializeTile()
        {
            tileParts = CurrentTileGO.GetComponent<TileParts>();
            
            var tileConfig = OnChainBoardDataConverter.GetTypeAndRotation(TileConfig);
            string id = OnChainBoardDataConverter.TileTypes[tileConfig.Item1];
            _rotation = (byte)((tileConfig.Item2 + 1) % 4);
            CurrentTileGO.GetComponent<TileRotator>().RotateTile((_rotation + 3) % 4);
            
            AllCityRenderers = new List<SpriteRenderer>();
            AllCityLineRenderers = new List<LineRenderer>();
            
            houseRenderers = tileParts.HouseRenderers;
            List<SpriteRenderer> arcRenderers = tileParts.ArcRenderers;
            TerritoryFiller territoryFiller = tileParts.TileTerritoryFiller;
            WallPlacer = tileParts.WallPlacer;
            List<Transform> pillars = null;
            Transform[] pins = tileParts.PinsPositions;
            if (WallPlacer != null)
            {
                pillars = WallPlacer.GetPillars().ToList();
            }

            if (pillars != null)
            {
                List<SpriteRenderer> pillarsRenderers = pillars.Select(x => x.GetComponent<SpriteRenderer>()).ToList();
                AllCityRenderers.AddRange(pillarsRenderers);
            }

            if (houseRenderers != null)
            { 
                AllCityRenderers.AddRange(houseRenderers);
                TileAssetsObject.BackIndex(houseRenderers.Count);
                
                foreach (var house in houseRenderers)
                {
                    int playerId = 0;
                    General.SessionManager sessionManager = General.SessionManager.Instance;
                    if (sessionManager == null || sessionManager.CurrentTurnPlayer == null)
                    {
                        playerId = Random.Range(0, 2);
                    }
                    else
                    {
                        playerId = sessionManager.CurrentTurnPlayer.LocalId;
                    }

                    if (_tileData.OwnerId == -1) playerId = -1;
                    int cityCount = TileConfig.Count(c => c == 'C');
                    house.gameObject.GetComponent<SpriteAnimator>()
                        .ChangeSprites(TileAssetsObject.GetNextHouse(playerId, cityCount == 1 ? true : false));
                }
            }

            if (arcRenderers != null)
            {
                AllCityRenderers.AddRange(arcRenderers);
            }

            
            if (WallPlacer != null)
            {
                WallPlacer.PlaceWall(false);
            }

            
            if (territoryFiller != null)
            {
                territoryFiller.PlaceTerritory(false);
            }
            
            if (pins != null && pins.Length > 0)
            {
                GenerateRoadPins(pins);
            }

            if (SessionManager.Instance.TileSelector.selectedPosition != null || _isTilePlacing)
            {
                FencePlacerForCloserToBorderCity(SessionManager.Instance.Board.CheckCityTileSidesToBorder(
                    _placingTilePosition.x,
                    _placingTilePosition.y));
                
                MinePlaceForCloserToBorderRoad(SessionManager.Instance.Board.CheckRoadTileSidesToBorder(_placingTilePosition.x,
                    _placingTilePosition.y));
            }
        }
        public void RecolorHouses(int playerId, bool isContest = false, int rotation = 0)
        {
            if (tileParts.HouseRenderers == null)
            {
                return;
            }

            if (isContest)
            {
                tileParts.ContestedWalls(rotation);
                //WallPlacer?.PlaceWall(true);
                // foreach (var border in tileParts.CloserToBorderFences)
                // {
                //     border.WallPlacer.PlaceWall(true);
                // }
                //
                // foreach (var arc in tileParts.ArcRenderers)
                // {
                //     arc.sprite = TileAssetsObject.StoneArc;
                // }
            }
            houseRenderers = CurrentTileGO.GetComponentsInChildren<SpriteRenderer>()
                .ToList().Where(x => x.name == "House").ToList();
            for (int i = 0; i < houseRenderers.Count; i++)
            {
                if (playerId == 3)
                {
                    houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().Play(TileAssetsObject.GetHouseByReference(houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().sprites));
                }
                else
                {
                    houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().Play(TileAssetsObject.GetHouseByReference(houseRenderers[i].gameObject.GetComponent<SpriteAnimator>().sprites, playerId));

                }
                TerritoryFiller territoryFiller = tileParts.TileTerritoryFiller;
                if (territoryFiller != null)
                {
                    territoryFiller.PlaceTerritory(isContest);
                }
            }
        }

        public void ChangeEnvironmentForContest()
        {
            //tileParts.ChangeEnvironmentForContest();
        }

        public void FencePlacerForCloserToBorderCity(List<Side> closerSides)
        {
            if (closerSides == null || tileParts.HouseRenderers == null) return;

            foreach (var side in closerSides)
            {
                if(_tileData.GetSide(side) != LandscapeType.City) continue;
                
                TileParts.CloserToBorderFence fence = tileParts.CloserToBorderFences.Find(x => x.Side == side);
                fence.Fence.SetActive(true);
                fence.WallPlacer.PlaceWall(false);
            }
        }

        public void MinePlaceForCloserToBorderRoad(List<Board.MineTileInfo> closerSides)
        {
            if (closerSides == null || !_tileData.IsRoad()) return;

            foreach (var side in closerSides)
            {
                if(_tileData.GetSide(side.Direction) != LandscapeType.Road) continue;

                foreach (var prefab in PrefabsManager.Instance.MineEnviromentTiles)
                {
                    if (prefab.Direction == side.Direction)
                    {
                        GameObject mine = Instantiate(prefab.MineTile, side.Position, Quaternion.identity,
                            SessionManager.Instance.Board.GetTileObject(side.TileBoardPosition.x, side.TileBoardPosition.y).transform);
                        side.Tile.GetComponent<TileGenerator>().RoadRenderer.gameObject.SetActive(false);
                    }
                }
            }
        }
        
        public void RecolorPins(int playerId)
        {
            if (Pins.Count == 0)
            {
                return;
            }
            foreach (var pin in Pins)
            {
                if (pin == null) continue;
                pin.SetPin(playerId);
            }
        }
        
        public void RecolorPinOnSide(int playerId, int side, bool isContest = false)
        {
            if (Pins.Count == 0)
            {
                return;
            }
            if (Pins[side] == null)
            {
                return;
            }
            Pins[side].SetPin(playerId, isContest);
        }
    }

    [Serializable]
    public class RoadPair
    {
        public string MainConfig;
        public string MirroredConfig;
        public Sprite Sprite;
        public bool FlipX;
        public bool FlipY;
        public GameObject RoadPath;
    }

    [Serializable]
    public class CityData
    {
        public string Config;
        public int Rotation;
        public GameObject CityPrefab;
    }

    [Serializable]
    public class TilePrefab
    {
        public string Config;
        public GameObject TilePrefabGO;
    }
}