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
        public TileParts tileParts;
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
        
        private List<TileParts.HouseGameObject> _housesParent = new List<TileParts.HouseGameObject>();
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
                ? SessionManager.Instance.CurrentTurnPlayer.PlayerSide
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
            
            TileRotator currentGoTileRotator = CurrentTileGO.GetComponent<TileRotator>();
            AllCityRenderers = new List<SpriteRenderer>();
            AllCityLineRenderers = new List<LineRenderer>();
            _housesParent = tileParts.Houses;
            
            List<SpriteRenderer> arcRenderers = tileParts.ArcRenderers;
            TerritoryFiller territoryFiller = tileParts.TileTerritoryFiller;
            WallPlacer = tileParts.WallPlacer;
            List<Transform> pillars = null;
            
            if (WallPlacer != null)
            {
                pillars = WallPlacer.GetPillars().ToList();
            }

            if (pillars != null)
            {
                List<SpriteRenderer> pillarsRenderers = pillars.Select(x => x.GetComponent<SpriteRenderer>()).ToList();
                AllCityRenderers.AddRange(pillarsRenderers);
            }

            if (_housesParent != null)
            { 
                for (int i = 0; i < _housesParent.Count; i++)
                {
                    int playerId = 0;
                    TileParts.HouseGameObject house = _housesParent[i];
                    SessionManager sessionManager = SessionManager.Instance;
                    if (sessionManager == null || sessionManager.CurrentTurnPlayer == null)
                    {
                        playerId = Random.Range(0, 2);
                    }
                    else
                    {
                        playerId = sessionManager.CurrentTurnPlayer.PlayerSide;
                    }

                    if (_tileData.OwnerId == -1) playerId = -1;
                    GameObject prefab = _tileData.HouseSprites.Count > i 
                        ? PrefabsManager.Instance.GetNonContestedHousePrefabByReference(_tileData.HouseSprites[i], playerId)
                        : PrefabsManager.Instance.GetRandomNotContestedHouseGameObject(1, playerId);
                    GameObject houseObject = Instantiate(prefab, house.Parent.transform);
                    currentGoTileRotator.SimpleRotationObjects.Add(house.Parent.transform);
                    house.SetData(houseObject);
                }
                
                AllCityRenderers.AddRange(_housesParent.Select(x => x.HouseSpriteRenderer));
                
                // for(int i = 0; i < _tileData.HouseSprites.Count; i++){
                //     if (i >= _housesParent.Count) break;
                //     // houseRenderers[i].sprite = _tileData.HouseSprites[i];
                //     _housesParent[i].Parent.gameObject.SetActive(false);
                //     Transform newHouseParent = _housesParent[i].Parent.transform.parent;
                //     TileParts.HouseGameObject house = new TileParts.HouseGameObject(newHouseParent.gameObject); 
                //     GameObject houseObject = PrefabsManager.Instance.GetNonContestedHousePrefabByReference(_tileData.HouseSprites[i]);
                //     GameObject houseGO = Instantiate(houseObject, houseRenderers[i].transform.parent.parent);
                //     houseGO.transform.position = houseRenderers[i].transform.parent.position;
                //     house.SetData(houseGO);
                //     tileParts.Houses.Add(house);
                // }
            }
            
            foreach (var decoration in tileParts.DecorationsRenderers)
            {
                currentGoTileRotator.MirrorRotationObjects.Add(decoration.transform);
            }

            foreach (var area in tileParts.Areas)
            {
                currentGoTileRotator.LineRenderers.Add(area.lineRenderer);
            }
            
            currentGoTileRotator.RotateTile((_rotation + 3) % 4);
            Transform[] pins = tileParts.PinsPositions;
            
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
            
            tileParts.SpawnTileObjects();

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
            if (isContest)
            {
                tileParts.PlaceContestedWalls(rotation);
                tileParts.PlaceFlags(rotation, playerId == 3 ? _tileData.OwnerId : playerId);
            }
            
            //List<TileParts.HouseGameObject> activeHouses = tileParts.Houses.Where(x => x.Parent.gameObject.activeSelf).ToList();

            if (isContest)
            {
                if (tileParts.Houses.Count > 0)
                {
                    if (_tileData.IsCityParallel())
                    {
                        ReplaceHouses(tileParts.Houses.GetRange(0,2), playerId == 3 ? _tileData.OwnerId : playerId);
                        ReplaceHouses(tileParts.Houses.GetRange(2,2), playerId == 3 ? _tileData.OwnerId : playerId);
                    }
                    else if (tileParts.Houses.Count == 8)
                    {
                        MergeHouses(tileParts.Houses, playerId == 3 ? _tileData.OwnerId : playerId);
                    }
                    else
                    {
                        ReplaceHouses(tileParts.Houses, playerId == 3 ? _tileData.OwnerId : playerId);
                    }
                    
                }   
            }
            else
            {
                if (tileParts.Houses.Count == 0) return;
                
                foreach (var house in tileParts.Houses)
                {
                    GameObject housePrefab = PrefabsManager.Instance.GetRandomNotContestedHouseGameObject(1, playerId);
                    Transform prevHouse = house.HouseSpriteRenderer.transform.parent;
                    prevHouse.gameObject.SetActive(false);
                    GameObject houseGO = Instantiate(housePrefab, house.Parent.transform);
                    houseGO.transform.position = prevHouse.position;
                    house.SetData(houseGO);
                    
                }
            }
            
            TerritoryFiller territoryFiller = tileParts.TileTerritoryFiller;
            if (territoryFiller != null)
            {
                territoryFiller.PlaceTerritory(isContest);
            }
        }

        public void MergeHouses(List<TileParts.HouseGameObject> houses, int playerId)
        {
            
            int count = houses.Count;
            if (count == 0) return;

            Vector2 mergedPosition = Vector2.zero;
            foreach (var house in houses)
            {
                mergedPosition += (Vector2)house.Parent.transform.position;
            }
            mergedPosition /= count;
            foreach (var house in houses)
            {
                house.Parent.transform.position = mergedPosition;
                house.Parent.gameObject.SetActive(false);
            }
            TileParts.HouseGameObject mainHouse = houses[0];
            mainHouse.Parent.gameObject.SetActive(true);

            if (TileAssetsObject.IsContestedHouse(mainHouse.HouseSpriteRenderer.sprite, count / 2, playerId))
            {
                return;
            }
            
            Destroy(mainHouse.HouseSpriteRenderer.transform.parent.gameObject);
            GameObject mergedHousePrefab = PrefabsManager.Instance.GetContestedHouse(count/2, playerId);
            GameObject houseGO = Instantiate(mergedHousePrefab, mainHouse.Parent.transform);
            houseGO.transform.position = mergedPosition;
            mainHouse.SetData(houseGO);
        }

        public void ReplaceHouses(List<TileParts.HouseGameObject> houses, int playerId)
        {
            if (playerId == 3)
            {
                return;
            }
            foreach (var house in houses)
            {
                GameObject newHousePrefab = PrefabsManager.Instance.GetNonContestedHousePrefabByReference(house.HouseSpriteRenderer.sprite, playerId);
                house.HouseSpriteRenderer.transform.parent.gameObject.SetActive(false);
                GameObject houseGO = Instantiate(newHousePrefab, house.Parent.transform);
                houseGO.transform.localPosition = Vector3.zero;
                house.SetData(houseGO);
            }
        }

        public void ChangeEnvironmentForContest()
        {
            //tileParts.ChangeEnvironmentForContest();
        }

        public void FencePlacerForCloserToBorderCity(List<Side> closerSides)
        {
            if (closerSides == null || tileParts.Houses == null) return;
            List<Side> closerCitySide = new List<Side>();
            foreach (var side in closerSides)
            {
                if(_tileData.GetSide(side) != LandscapeType.City) continue;
                
                TileParts.CloserToBorderFence fence = tileParts.CloserToBorderFences.Find(x => x.Side == side);
                fence.Fence.SetActive(true);
                fence.WallPlacer.PlaceWall(false);
                closerCitySide.Add(side);
            }
            tileParts.SetContestedBorderWalls(closerCitySide);
            
        }

        public void MinePlaceForCloserToBorderRoad(List<BoardManager.MineTileInfo> closerSides)
        {
            if (closerSides == null || !_tileData.IsRoad()) return;

            foreach (var side in closerSides)
            {
                if(_tileData.GetSide(side.Direction) != LandscapeType.Road) continue;
                
                bool snowBoardPart = SessionManager.Instance.Board.IsSnowBoardPart(side.TileBoardPosition.x, side.TileBoardPosition.y);

                foreach (var prefab in PrefabsManager.Instance.MineEnviromentTiles)
                {
                    if (prefab.Direction == side.Direction && prefab.BoardPart == (snowBoardPart ? 3 : 0))
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