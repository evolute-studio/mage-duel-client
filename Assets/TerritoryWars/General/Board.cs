using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerritoryWars.Dojo;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.ScriptablesObjects;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace TerritoryWars.General
{
    public class ValidPlacement
    {
        public int x { get; set; }
        public int y { get; set; }
        public int rotation { get; set; }
        
        public ValidPlacement(int x, int y, int rotation)
        {
            this.x = x;
            this.y = y;
            this.rotation = rotation;
        }

        public ValidPlacement(int x, int y)
        {
            this.x = x;
            this.y = y;
            rotation = 0;
        }
        
        public ValidPlacement(Vector2Int vector)
        {
            x = vector.x;
            y = vector.y;
            rotation = 0;
        }
        public int GetHashCode(ValidPlacement obj)
        {
            return HashCode.Combine(obj.x, obj.y, obj.rotation);
        }
    }

    public class Board : MonoBehaviour
    {
        public TileAssetsObject tileAssets => PrefabsManager.Instance.TileAssetsObject;

        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private static float tileSpacing = 0.965f;

        private GameObject[,] tileObjects;
        private TileData[,] tileData;
        
        public Dictionary<Vector2Int, TileData> PlacedTiles = new Dictionary<Vector2Int, TileData>();

        public delegate void TilePlaced(TileData tile, int x, int y);
        public event TilePlaced OnTilePlaced;

        public void Initialize()
        {
            InitializeBoard();
            var onChainBoard = DojoGameManager.Instance.DojoSessionManager.LocalPlayerBoard;
            char[] edgeTiles = OnChainBoardDataConverter.GetInitialEdgeState(onChainBoard.initial_edge_state);
            CreateBorder(edgeTiles);
            SessionManager.Instance.CloudsController.SetMountains(GetMountains());
        }

        private void InitializeBoard()
        {
            tileObjects = new GameObject[width, height];
            tileData = new TileData[width, height];
        }
        
        private void CreateBorder(char[] border)
        {
            GenerateBorderSide(new Vector2Int(0, 0), new Vector2Int(9, 0), 0, border[0..8],true);
            GenerateBorderSide(new Vector2Int(9, 0), new Vector2Int(9, 9), 3, border[8..16]);
            GenerateBorderSide(new Vector2Int(9, 9), new Vector2Int(0, 9), 2, border[16..24]);
            GenerateBorderSide(new Vector2Int(0, 9), new Vector2Int(0, 0), 1, border[24..32], true);
        }
        
        public void GenerateBorderSide(Vector2Int startPos, Vector2Int endPos, int rotationTimes, char[] border, bool swapOrderLayer = false)
        {
            string roadTile = "FFFR";
            string cityTile = "FFFC";
            string fieldTile = "FFFF";
            // eg Start (9, 9) end (9, 0)

            roadTile = TileData.GetRotatedConfig(roadTile, rotationTimes);
            cityTile = TileData.GetRotatedConfig(cityTile, rotationTimes);

            string[] tilesToSpawn = new string[8];
            for (int i = 0; i < tilesToSpawn.Length; i++)
            {
                tilesToSpawn[i] = border[i] switch {
                    'R' => roadTile,
                    'C' => cityTile,
                    'F' => fieldTile,
                    _ => fieldTile
                };
            }

            List<Vector2Int> availablePositions = new List<Vector2Int>();
            if (endPos.y != startPos.y)
            {
                for (int i = startPos.y + 1; i < endPos.y; i++)
                {
                    availablePositions.Add(new Vector2Int(startPos.x, i));
                }
                for (int i = startPos.y - 1; i > endPos.y; i--)
                {
                    availablePositions.Add(new Vector2Int(startPos.x, i));
                }
                
            }
            else
            {
                for (int i = startPos.x + 1; i < endPos.x; i++)
                {
                    availablePositions.Add(new Vector2Int(i, startPos.y));
                }
                for (int i = startPos.x - 1; i > endPos.x; i--)
                {
                    availablePositions.Add(new Vector2Int(i, startPos.y));
                }
                
            }

            // Place forests only at (0,9) and (9,0), mountains at other corners
            if (startPos.x == 0 && startPos.y == 9)
            {
                PlaceTile(new TileData(fieldTile), 0, 9, -1);
                GameObject forest = Instantiate(tileAssets.ForestPrefab, transform.position, Quaternion.identity, tileObjects[0, 9].transform);
                forest.transform.localPosition = Vector3.zero;
                GameObject spawnedTile = tileObjects[0, 9];
                Destroy(spawnedTile);
            }
            else if (startPos.x == 9 && startPos.y == 0)
            {
                PlaceTile(new TileData(fieldTile), 9, 0, -1);
                GameObject forest = Instantiate(tileAssets.ForestPrefab, transform.position, Quaternion.identity, tileObjects[9, 0].transform);
                forest.transform.localPosition = Vector3.zero;
                forest.transform.localScale = new Vector3(-1, 1, 1);
                GameObject spawnedTile = tileObjects[9, 0];
                Destroy(spawnedTile);
            }
            else
            {
                if(GetTileObject(startPos.x, startPos.y) != null)
                    Destroy(GetTileObject(startPos.x, startPos.y));
                // Place mountains at start and end positions
                PlaceTile(new TileData(fieldTile), startPos.x, startPos.y, -1);
                // Don't place forests, only mountains at other corners
                GameObject mountain;
                if (IsSnowBoardPart(startPos.x, startPos.y))
                {
                    mountain = Instantiate(PrefabsManager.Instance.GetRandomSnowMountainTile(),
                        tileObjects[startPos.x, startPos.y].transform.Find("RoadRenderer"));
                }
                else
                {
                    mountain = Instantiate(PrefabsManager.Instance.GetRandomMountainTile(),
                        tileObjects[startPos.x, startPos.y].transform.Find("RoadRenderer"));
                }

                foreach (var mountainSprite in mountain.GetComponentsInChildren<SpriteRenderer>())
                {
                    mountainSprite.sortingOrder = 14;
                }
                if (swapOrderLayer)
                {
                    foreach (var mountainSprite in mountain.GetComponentsInChildren<SpriteRenderer>())
                    {
                        mountainSprite.sortingOrder = 14; // 20
                    }
                }

                tileObjects[startPos.x, startPos.y].GetComponentsInChildren<TileParts>().ToList().ForEach(
                    renderer =>
                    {
                        renderer.Enviroment.SetActive(false);
                    });
                
                tileObjects[startPos.x, startPos.y].GetComponentInChildren<TileParts>().HideForestAreas();
            }

            if (endPos != new Vector2Int(9, 0) && endPos != new Vector2Int(0, 9))
            {
                if(GetTileObject(endPos.x, endPos.y) != null)
                    Destroy(GetTileObject(endPos.x, endPos.y));
                PlaceTile(new TileData(fieldTile), endPos.x, endPos.y, -1);
                GameObject mountain;
                if (IsSnowBoardPart(endPos.x, endPos.y))
                {
                    mountain = Instantiate(PrefabsManager.Instance.GetRandomSnowMountainTile(),
                        tileObjects[endPos.x, endPos.y].transform.Find("RoadRenderer"));
                }
                else
                {
                    mountain = Instantiate(PrefabsManager.Instance.GetRandomMountainTile(),
                        tileObjects[endPos.x, endPos.y].transform.Find("RoadRenderer"));
                }

                foreach (var mountainSprite in mountain.GetComponentsInChildren<SpriteRenderer>())
                {
                    mountainSprite.sortingOrder = 14;
                }
                if (swapOrderLayer)
                {
                    foreach (var mountainSprite in mountain.GetComponentsInChildren<SpriteRenderer>())
                    {
                        mountainSprite.sortingOrder = 14; // 20
                    }
                }
                tileObjects[endPos.x, endPos.y].GetComponentsInChildren<TileParts>().ToList().ForEach(
                    renderer => 
                    {
                        renderer.Enviroment.SetActive(false);
                    });
                tileObjects[endPos.x, endPos.y].GetComponentInChildren<TileParts>().HideForestAreas();
            }

            for (int i = 0; i < availablePositions.Count; i++)
            {
                TileData tile = new TileData(tilesToSpawn[i]);
                PlaceTile(tile, availablePositions[i].x, availablePositions[i].y, -1);
                if (tilesToSpawn[i] == fieldTile)
                {
                    tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("RoadRenderer")
                            .GetComponent<SpriteRenderer>().sprite
                        = null;
                    
                    GameObject mountain;
                    if (IsSnowBoardPart(availablePositions[i].x, availablePositions[i].y))
                    {
                        mountain = Instantiate(PrefabsManager.Instance.GetRandomSnowMountainTile(),
                            tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("RoadRenderer"));
                    }
                    else
                    {
                        mountain = Instantiate(PrefabsManager.Instance.GetRandomMountainTile(),
                            tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("RoadRenderer"));
                    }

                    foreach (var mountainSprite in mountain.GetComponentsInChildren<SpriteRenderer>())
                    {
                        mountainSprite.sortingOrder = 14;
                    }
                    if (swapOrderLayer)
                    {
                        foreach (var mountainSprite in mountain.GetComponentsInChildren<SpriteRenderer>())
                        {
                            mountainSprite.sortingOrder = 14; // 20
                        }
                    }
                    TileParts tileParts = tileObjects[availablePositions[i].x, availablePositions[i].y]
                        .GetComponentInChildren<TileParts>();
                    foreach (var area in tileParts.Areas)
                    {
                        if (area == null) continue;
                        Destroy(area.gameObject);
                    }
                    tileParts.Areas.Clear();
                }
                else if (tilesToSpawn[i] == roadTile)
                {
                    tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("BorderArc").gameObject.SetActive(true);
                    Transform borderArc = tileObjects[availablePositions[i].x, availablePositions[i].y].transform.Find("BorderArc");
                    TileRotator.GetMirrorRotationStatic(borderArc, rotationTimes);
                    var pins = tileObjects[availablePositions[i].x, availablePositions[i].y]
                        .GetComponent<TileGenerator>().Pins;
                    tileObjects[availablePositions[i].x, availablePositions[i].y].GetComponentsInChildren<TileParts>().ToList().ForEach(renderer =>
                    {
                        renderer.Mill.SetActive(false);
                        renderer.Forest[rotationTimes].SetActive(true);
                        if (swapOrderLayer)
                        {
                            renderer.Forest[rotationTimes].GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(sr =>
                            {
                                if(sr.gameObject.name == "RoadRenderer") return;
                                sr.sortingOrder = 20;
                            });
                        }
                    });
                    foreach (var pin in pins)
                    {
                        if (pin == null) continue;
                        pin.Initialize(-1, 1);
                    }
                }
                else if (tilesToSpawn[i] == cityTile)
                {
                    tileObjects[availablePositions[i].x, availablePositions[i].y].GetComponentsInChildren<TileParts>().ToList().ForEach(renderer =>
                    {
                        renderer.Forest[rotationTimes].SetActive(true);
                        if (swapOrderLayer)
                        {
                            renderer.Forest[rotationTimes].GetComponentsInChildren<SpriteRenderer>().ToList().ForEach(sr =>
                            {
                                sr.sortingOrder = 20;
                            });
                        }
                    });
                }
                
                tileObjects[availablePositions[i].x, availablePositions[i].y].GetComponentsInChildren<TileParts>().ToList().ForEach(
                    renderer =>
                    {
                        renderer.Enviroment.SetActive(false);
                    });
            }
        }
        
        public bool PlaceTile(TileData data, int x, int y, int ownerId)
        {
            CustomLogger.LogImportant($"Placing tile {data.id} and sides {data.sides[0]}{data.sides[1]}{data.sides[2]}{data.sides[3]} at {x}, {y}");
            // if (!CanPlaceTile(data, x, y))
            // {
            //     CustomLogger.LogWarning($"Can't place tile {data.id} at {x}, {y}");
            //     return false;
            // }

            tileData[x, y] = data;
            data.OwnerId = ownerId;
            GameObject tile = Instantiate(tilePrefab, GetTilePosition(x, y), Quaternion.identity, transform);
            tile.name += $"_{x}_{y}";
            tile.GetComponent<TileGenerator>().Generate(data, true, new Vector2Int(x,y));
            tile.GetComponent<TileView>().UpdateView(data);
            tileObjects[x, y] = tile;
            PlacedTiles[new Vector2Int(x, y)] = data;
            OnTilePlaced?.Invoke(data, x, y);
            CheckConnections(data, x, y);
            
            if( (x == 1 || x == width - 2 || y == 1 || y == height - 2) && !IsEdgeTile(x, y))
            {
                TryConnectEdgeStructure(ownerId, x, y);
            }

            return true;
        }
        
        public void FloatingTextAnimation(Vector2Int position)
        {
            GameObject gameObject = SessionManager.Instance.Board.GetTileObject(position.x, position.y);
            TileData tileData = SessionManager.Instance.Board.GetTileData(position.x, position.y);
            TileParts tileParts = gameObject.GetComponentInChildren<TileParts>();
            TileGenerator tileGenerator = gameObject.GetComponent<TileGenerator>();
            int playerId = SessionManager.Instance.CurrentTurnPlayer.SideId;
            playerId = SetLocalPlayerData.GetLocalIndex(playerId);
            string side = playerId == 0 ? "blue" : "red";
            Vector3 motion = new Vector3(0, 0.3f, 0);
                    
            foreach (var house in tileParts.Houses)
            {
                float duration = 2f + UnityEngine.Random.Range(0f, 1f);
                Vector3 textPosition = house.HouseSpriteRenderer.transform.position;
                FloatingTextManager.Instance.Show("+1", textPosition, motion, duration, "house_icon_" + side);
            }
            foreach (var pin in tileGenerator.Pins)
            {
                if(pin == null) continue;
                float duration = 2f + UnityEngine.Random.Range(0f, 1f);
                Vector3 textPosition = pin.transform.position;
                int cityCount = tileData.id.Count(c => c == 'R');
                string messageText = cityCount == 2 ? "+2" : "+1";
                FloatingTextManager.Instance.Show(messageText, textPosition, motion, duration, "road_icon_" + side);
                if(cityCount == 2) break;
            }
        }

        public void ScoreClientPrediction(int playerIndex, TileData data)
        {
            if(!SessionManager.Instance.IsLocalPlayerTurn) return;
            SessionManager.Instance.gameUI.playerInfoUI.AddClientCityScore(playerIndex, data.id.Count(c => c == 'C') * 2);
            SessionManager.Instance.gameUI.playerInfoUI.AddClientRoadScore(playerIndex, data.id.Count(r => r == 'R'));
        }

        public bool RevertTile(int x, int y)
        {
            Destroy(tileObjects[x,y]);
            tileObjects[x, y] = null;
            tileData[x, y] = null;
            PlacedTiles.Remove(new Vector2Int(x, y));

            if(tileObjects[x,y] == null && tileData[x,y] == null && PlacedTiles.ContainsKey(new Vector2Int(x, y)))
            {
                return true;
            }
            
            return false;
        }
        
        public enum StructureType
        {
            City,
            Road,
            All,
            None,
        }

        public void CheckAndConnectEdgeStructure(int ownerId, int x, int y, StructureType type, bool isCityContest = false, bool isRoadContest = false)
        {
            if( (x == 1 || x == width - 2 || y == 1 || y == height - 2) && !IsEdgeTile(x, y))
            {
                TryConnectEdgeStructure(ownerId, x, y, type, isCityContest, isRoadContest);
            }
        }

        public void ConnectEdgeStructureAnimation(int ownerId, TileData tileData, int x, int y, bool isCityContest = false, bool isRoadContest = false)
        {
            if(isCityContest || isRoadContest || SessionManager.Instance.IsSessionStarting) return;
            FloatingTextAnimation(new Vector2Int(x, y));
            ScoreClientPrediction(ownerId, tileData);
        }

        private void TryConnectEdgeStructure(int owner, int x, int y, StructureType type = StructureType.All, bool isCityContest = false, bool isRoadContest = false)
        {
            GameObject[] neighborsGO = new GameObject[4];
            TileData[] neighborsData = new TileData[4];
            int[][] positions = new[] { new int[] {1, 0}, new int[] {0, -1}, new int[] {-1, 0}, new int[] {0, 1} };
            int[][] tilePositions = new int[4][];
            for (int index = 0; index < 4; index++)
            {
                tilePositions[index] = new int[2];
            }

            for (int i = 0; i < 4; i++)
            {
                int newX = x + positions[i][0];
                int newY = y + positions[i][1];
                tilePositions[i][0] = newX;
                tilePositions[i][1] = newY;
                neighborsGO[i] = tileObjects[newX, newY];
                neighborsData[i] = tileData[newX, newY];
            }
            
            for( int i = 0; i < 4; i++)
            {
                if(IsEdgeTile(tilePositions[i][0], tilePositions[i][1]) && neighborsGO[i] != null)
                {
                    ConnectEdgeStructureAnimation(owner, neighborsData[i], tilePositions[i][0], tilePositions[i][1], isCityContest, isRoadContest);
                    TileGenerator tileGenerator = neighborsGO[i].GetComponent<TileGenerator>();
                    if (type == StructureType.All || type == StructureType.City)
                    {
                        (_, byte rotation) = OnChainBoardDataConverter.GetTypeAndRotation(neighborsData[i].id);
                        tileGenerator.RecolorHouses(owner, isCityContest, rotation);
                    }

                    if (type == StructureType.All || type == StructureType.Road)
                    {
                        TileGenerator placedTileGenerator = tileObjects[x, y].GetComponent<TileGenerator>();
                        List<Side> roadSides = neighborsData[i].GetRoadSides();
                        if (roadSides.Count == 0) continue;

                        if (isRoadContest)
                        {
                            foreach (var road in tileGenerator.CurrentTileGO.GetComponent<TileParts>().RoadRenderers)
                            {
                                if (road != null)
                                {
                                    road.sprite = PrefabsManager.Instance.TileAssetsObject.GetContestedRoadByReference(road.sprite);
                                }
                            }
                        }
                        
                        for (int j = 0; j < tileGenerator.Pins.Count; j++)
                        {
                            if (tileGenerator.Pins[j] == null) continue;
                            Side oppositeSide = GetOppositeSide((Side)j);
                            RoadPin placedTilePin = placedTileGenerator.Pins[(int)oppositeSide];
                            if (placedTilePin == null) continue;
                            int ownerToSet = placedTilePin.OwnerId;
                            bool isContested = placedTilePin.IsContested;
                            tileGenerator.Pins[j].SetPin(ownerToSet, isContested);
                            
                        }
                    }
                }
            }
        }
        
        public bool IsSnowBoardPart(int x, int y)
        {
            if (x > 4 & y > 4)
            {
                return true; // snow mountain
            }

            return false;
        }

        public Vector2Int GetEdgeNeighbors(int x, int y, Side side)
        {
            //Side oppositeSide = GetOppositeSide(side);
            int newX = x + GetXOffset(side);
            int newY = y + GetYOffset(side);
            if (IsValidPosition(newX, newY) && IsEdgeTile(newX, newY))
            {
                return new Vector2Int(newX, newY);
            }
            return new Vector2Int(-1, -1);
        }
        
        public void CloseAllStructures()
        {
            for(int x = 0; x < width; x++)
            {
                for(int y = 0; y < height; y++)
                {
                    GameObject tile = GetTileObject(x, y);
                    if(tile == null || tile.TryGetComponent(out TileGenerator tryGetTileGenerator)) continue;
                    TileGenerator tileGenerator = tile.GetComponent<TileGenerator>();
                    List<Side> sides = CheckCityTileSidesToEmpty(x, y);
                    tileGenerator.FencePlacerForCloserToBorderCity(sides);
                }
            }
        }

        public void CloseCityStructure(byte root)
        {
            var city = DojoGameManager.Instance.DojoSessionManager.GetCityByPosition(root);

            foreach (var node in city.Value)
            {
                Vector2Int position = OnChainBoardDataConverter.GetPositionByRoot(node.position);
                GameObject tile = GetTileObject(position.x, position.y);
                if (tile == null || !tile.TryGetComponent(out TileGenerator tileGenerator)) continue;
                List<Side> sides = CheckCityTileSidesToEmpty(position.x, position.y);
                tileGenerator.FencePlacerForCloserToBorderCity(sides);
            }
        }

        public List<Side> CheckCityTileSidesToBorder(int x, int y)
        {
            // returns list int of sides that are closer to the border 
            // 0 - TOP
            // 1 - Right
            // 2 - Bottom
            // 3 - Left
            
            List<Side> closerSides = new List<Side>();
            if(IsEdgeTile(x,y)) return closerSides;

            if (IsEdgeTile(x + 1, y) && !GetTileData(x + 1, y).IsCity()) { closerSides.Add(Side.Top); }

            if (IsEdgeTile(x,y - 1) && !GetTileData(x, y - 1).IsCity()) { closerSides.Add(Side.Right); }

            if (IsEdgeTile(x - 1, y) && !GetTileData(x - 1, y).IsCity()) { closerSides.Add(Side.Bottom); }

            if (IsEdgeTile(x, y + 1) && !GetTileData(x, y + 1).IsCity()) { closerSides.Add(Side.Left); }

            return closerSides;
        }
        
        public List<Side> CheckCityTileSidesToEmpty(int x, int y)
        {
            // returns list int of sides that are closer to the border 
            // 0 - TOP
            // 1 - Right
            // 2 - Bottom
            // 3 - Left
            
            List<Side> closerSides = new List<Side>();
            if(IsEdgeTile(x,y)) return closerSides;

            if (GetTileData(x + 1, y) == null) { closerSides.Add(Side.Top); }

            if (GetTileData(x, y - 1) == null) { closerSides.Add(Side.Right); }

            if (GetTileData(x - 1, y) == null) { closerSides.Add(Side.Bottom); }

            if (GetTileData(x, y + 1) == null) { closerSides.Add(Side.Left); }

            return closerSides;
        }

        public List<GameObject> GetMountains()
        {
            List<GameObject> mountains = new List<GameObject>();
            
            for (int i = 0; i < 10; i++)
            {
                if (tileData[i, 0].id == "FFFF" && !mountains.Contains(tileObjects[i, 1]))
                {
                    mountains.Add(tileObjects[i, 0]);
                }
            }
            
            for (int i = 0; i < 10; i++)
            {
                if (tileData[9, i].id == "FFFF" && !mountains.Contains(tileObjects[9, i]))
                {
                    mountains.Add(tileObjects[9, i]);
                }
            }

            for (int i = 9; i >= 0 ; i--)
            {
                if (tileData[i, 9].id == "FFFF" && !mountains.Contains(tileObjects[i, 9]))
                {
                    mountains.Add(tileObjects[i, 9]);
                }
            }
            
            for (int i = 9; i >= 0 ; i--)
            {
                if (tileData[0, i].id == "FFFF" && !mountains.Contains(tileObjects[0, i]))
                {
                    mountains.Add(tileObjects[0, i]);
                }
            }
            return mountains;
        }
        
        public List<MineTileInfo> CheckRoadTileSidesToBorder(int x, int y)
        {
            // returns list int of sides that are closer to the border 
            // 0 - TOP
            // 1 - Right
            // 2 - Bottom
            // 3 - Left
            
            List<MineTileInfo> closerSides = new List<MineTileInfo>();
            if(IsEdgeTile(x,y)) return closerSides;

            if (IsEdgeTile(x + 1, y) && !GetTileData(x + 1, y).IsRoad())
            {
                closerSides.Add(new MineTileInfo
                {
                    Tile = GetTileObject(x + 1, y),
                    Position = GetTilePosition(x + 1, y),
                    TileBoardPosition = new Vector2Int(x + 1, y),
                    Direction = Side.Top
                });
            }

            if (IsEdgeTile(x, y - 1) && !GetTileData(x, y - 1).IsRoad())
            {
                closerSides.Add(new MineTileInfo
                {
                    Tile = GetTileObject(x, y - 1),
                    Position = GetTilePosition(x, y - 1),
                    TileBoardPosition = new Vector2Int(x, y - 1),
                    Direction = Side.Right
                });
            }

            if (IsEdgeTile(x - 1, y) && !GetTileData(x - 1, y).IsRoad())
            {
                closerSides.Add(new MineTileInfo
                {
                    Tile = GetTileObject(x - 1, y),
                    Position = GetTilePosition(x - 1, y),
                    TileBoardPosition = new Vector2Int(x - 1, y),
                    Direction = Side.Bottom
                });
            }

            if (IsEdgeTile(x, y + 1) && !GetTileData(x, y + 1).IsRoad())
            {
                closerSides.Add(new MineTileInfo
                {
                    Tile = GetTileObject(x, y + 1),
                    Position = GetTilePosition(x, y + 1),
                    TileBoardPosition = new Vector2Int(x, y + 1),
                    Direction = Side.Left
                });
            }

            return closerSides;
        }
        
        public bool IsEdgeTile(int x, int y)
        {
            return x == 0 || x == width - 1 || y == 0 || y == height - 1;
        }

        public (Vector2Int, Side) GetNeighborPositionAndSideToEdgeTile(int x, int y)
        {
            if (x == 0)
            {
                return (new Vector2Int(1, y), Side.Bottom);
            }
            else if (x == width - 1)
            {
                return (new Vector2Int(width - 2, y), Side.Top);
            }
            else if (y == 0)
            {
                return (new Vector2Int(x, 1), Side.Right);
            }
            else if (y == height - 1)
            {
                return (new Vector2Int(x, height - 2), Side.Left);
            }
            return (new Vector2Int(-1, -1), Side.None);
        }
        
        public bool CanPlaceTile(TileData tile, int x, int y)
        {
            
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return false;
            }

            
            if (tileData[x, y] != null)
            {
                return false;
            }

            
            int placedTiles = 0;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (tileData[i, j] != null)
                    {
                        placedTiles++;
                    }
                }
            }

            
            if (placedTiles < 36)
            {
                return true;
            }

            
            Dictionary<Side, TileData> neighbors = new Dictionary<Side, TileData>();
            bool hasAnyNeighbor = false;
            bool hasNonBorderNeighbor = false;
            bool hasBorderWithNonField = false;

            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + GetXOffset(side);
                int newY = y + GetYOffset(side);

                if (IsValidPosition(newX, newY) && tileData[newX, newY] != null)
                {
                    neighbors[side] = tileData[newX, newY];
                    hasAnyNeighbor = true;

                    if (IsBorderTile(newX, newY))
                    {
                        
                        LandscapeType borderSide = tileData[newX, newY].GetSide(GetOppositeSide(side));
                        if (borderSide != LandscapeType.Field)
                        {
                            hasBorderWithNonField = true;
                        }
                    }
                    else
                    {
                        hasNonBorderNeighbor = true;
                    }
                }
            }

            
            if (!hasAnyNeighbor)
            {
                return false;
            }

            
            if (hasBorderWithNonField)
            {
                
                foreach (var neighbor in neighbors)
                {
                    Side side = neighbor.Key;
                    TileData adjacentTile = neighbor.Value;
                    if (!IsMatchingLandscape(tile.GetSide(side), 
                        adjacentTile.GetSide(GetOppositeSide(side))))
                    {
                        return false;
                    }
                }
                return true;
            }

            
            if (!hasNonBorderNeighbor)
            {
                return false;
            }

            
            foreach (var neighbor in neighbors)
            {
                Side side = neighbor.Key;
                TileData adjacentTile = neighbor.Value;

                LandscapeType currentSide = tile.GetSide(side);
                LandscapeType adjacentSide = adjacentTile.GetSide(GetOppositeSide(side));

                
                if (IsBorderTile(x + GetXOffset(side), y + GetYOffset(side)) && adjacentSide == LandscapeType.Field)
                {
                    continue;
                }

                
                if (!IsMatchingLandscape(currentSide, adjacentSide))
                {
                    return false;
                }
            }

            return true;
        }

        public void CheckConnections(TileData tile, int x, int y)
        {
            // Checking the boundaries of the field
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return;
            }

            // find all the neighboring tiles
            Dictionary<Side, TileData> neighbors = new Dictionary<Side, TileData>();
            bool hasAnyNeighbor = false;
            bool hasNonBorderNeighbor = false;

            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + GetXOffset(side);
                int newY = y + GetYOffset(side);

                if (IsValidPosition(newX, newY) && tileData[newX, newY] != null)
                {
                    neighbors[side] = tileData[newX, newY];
                    hasAnyNeighbor = true;

                    // Check whether it is not a borderline tile
                    if (!IsBorderTile(newX, newY))
                    {
                        hasNonBorderNeighbor = true;
                    }
                }
            }
            
            if (!hasAnyNeighbor)
            {
                return;
            }
            
            if (neighbors.Count == 1 && !hasNonBorderNeighbor)
            {
                var neighbor = neighbors.First();
                if (neighbor.Value.GetSide(GetOppositeSide(neighbor.Key)) == LandscapeType.Field)
                {
                    return;
                }
            }
            
            foreach (var neighbor in neighbors)
            {
                Side side = neighbor.Key;
                TileData adjacentTile = neighbor.Value;

                LandscapeType currentSide = tile.GetSide(side);
                LandscapeType adjacentSide = adjacentTile.GetSide(GetOppositeSide(side));
                if (!IsMatchingLandscape(currentSide, adjacentSide))
                {
                    continue;
                }
                
                if (currentSide == LandscapeType.City && adjacentSide == LandscapeType.City)
                {
                    
                }

                if (currentSide == LandscapeType.Road && adjacentSide == LandscapeType.Road)
                {
                    
                }
            }
        }
        

        private bool IsMatchingLandscape(LandscapeType type1, LandscapeType type2)
        {
            bool matches = (type1, type2) switch
            {
                (LandscapeType.City, LandscapeType.City) => true,
                (LandscapeType.Road, LandscapeType.Road) => true,
                (LandscapeType.Field, LandscapeType.Field) => true,
                _ => false
            };

            return matches;
        }

        public Side GetOppositeSide(Side side)
        {
            return side switch
            {
                Side.Top => Side.Bottom,
                Side.Right => Side.Left,
                Side.Bottom => Side.Top,
                Side.Left => Side.Right,
                Side.None => Side.None,
                //_ => throw new System.ArgumentException($"Invalid side: {side}")
            };
        }

        public int GetXOffset(Side dir)
        {
           // In isometric projection:
            // x increases when moving up (Top)
            // x decreases when moving down (Bottom)
            return dir switch
            {
                Side.Top => 1,
                Side.Bottom => -1,
                _ => 0
            };
        }

        public int GetYOffset(Side dir)
        {
            // In isometric projection:
            // y increases when moving to the left (Left)
            // y decreases when moving to the right (Right)
            return dir switch
            {
                Side.Left => 1,
                Side.Right => -1,
                _ => 0
            };
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < width && y >= 0 && y < height;
        }

        public bool IsBorderTile(int x, int y)
        {
            // Check whether the tile is on the field border
            return x == 0 || x == width - 1 || y == 0 || y == height - 1;
        }

        public int Width => width;
        public int Height => height;

        public static Vector3 GetTilePosition(int x, int y)
        {
            float xPosition = (x - y) * tileSpacing;
            float yPosition = (x + y) * (tileSpacing / 2);
            return new Vector3(xPosition, yPosition, 0);
        }
        
        public static Vector2Int GetPositionByWorld(Vector3 worldPosition)
        {
            float x = (worldPosition.x + worldPosition.y) / tileSpacing;
            float y = (worldPosition.y - worldPosition.x) / tileSpacing * 2;
            return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
        }
        
        public Vector2Int GetPositionByObject(GameObject tile)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tileObjects[x, y] == tile)
                    {
                        return new Vector2Int(x, y);
                    }
                }
            }

            return new Vector2Int(-1, -1);
        }
        
        public List<ValidPlacement> GetValidPlacements(TileData tile)
        {
            List<ValidPlacement> validPlacements = new List<ValidPlacement>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int rotation = 0; rotation < 4; rotation++)
                    {
                        if (CanPlaceTile(tile, x, y))
                        {
                            validPlacements.Add(new ValidPlacement(x, y, rotation));
                        }
                        tile.Rotate();
                    }

                    tile.Rotate(4 - (tile.rotationIndex % 4));
                }
            }

            return validPlacements;
        }

        public List<ValidPlacement> GetJokerValidPlacements()
        {
            // it's means all empty positions where exist at least one neighbor
            List<ValidPlacement> validPlacements = new List<ValidPlacement>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (tileData[x, y] != null) continue;
                    if (HasNeighbor(x, y))
                    {
                        validPlacements.Add(new ValidPlacement(x, y, 0));
                    }
                }
            }
            return validPlacements;
        }
        
        public bool HasNeighbor(int x, int y)
        {
            for (int i = 0; i < 4; i++)
            {
                int newX = x + GetXOffset((Side)i);
                int newY = y + GetYOffset((Side)i);
                if (IsValidPosition(newX, newY) && tileData[newX, newY] != null && !IsBorderTile(newX, newY))
                {
                    return true;
                }
            }

            return false;
        }

        public List<int> GetValidRotations(TileData tile, int x, int y)
        {
            List<int> validRotations = new List<int>();

            
            int initialRotation = tile.rotationIndex;

            
            for (int rotation = 0; rotation < 4; rotation++)
            {
                if (CanPlaceTile(tile, x, y))
                {
                    validRotations.Add(rotation);
                }
                tile.Rotate();
            }

            
            tile.Rotate(4 - (tile.rotationIndex % 4));
            while (tile.rotationIndex != initialRotation)
            {
                tile.Rotate();
            }

            return validRotations;
        }

        public GameObject GetTileObject(int x, int y)
        {
            return tileObjects[x, y];
        }

        public TileData GetTileData(int x, int y)
        {
            if (!IsValidPosition(x, y)) return null;
            return tileData[x, y];
        }

        public class RoadStructure
        {
            public Vector2Int tilePosition;
            public bool[] roadSides = new bool[4];
        }

        public class MineTileInfo
        {
            public GameObject Tile;
            public Vector3 Position;
            public Vector2Int TileBoardPosition;
            public Side Direction;
        }

        // public void OnGUI()
        // {
        //     // Create semi-transparent dark background style
        //     GUIStyle darkStyle = new GUIStyle(GUI.skin.label);
        //     darkStyle.normal.background = new Texture2D(1, 1);
        //     darkStyle.normal.background.SetPixel(0, 0, new Color(0, 0, 0, 0.5f));
        //     darkStyle.normal.background.Apply();
        //     darkStyle.normal.textColor = Color.white;
        //
        //     for (int x = 0; x < width; x++)
        //     {
        //         for (int y = 0; y < height; y++)
        //         {
        //             if (tileObjects[x, y] != null && tileData[x, y] != null)
        //             {
        //                 Vector3 screenPos = Camera.main.WorldToScreenPoint(tileObjects[x, y].transform.position);
        //                 screenPos.y = Screen.height - screenPos.y; // Конвертуємо Y координату для GUI
        //                 
        //                 // Background rect that covers all labels
        //                 GUI.Box(new Rect(screenPos.x - 20, screenPos.y - 50, 100, 60), "", darkStyle);
        //
        //                 GUI.Label(new Rect(screenPos.x - 20, screenPos.y - 10, 100, 20), tileData[x, y].id);
        //                 GUI.Label(new Rect(screenPos.x - 20, screenPos.y - 30, 100, 20), tileData[x, y].OwnerId.ToString());
        //                 string houseRenderers = "";
        //                 foreach (var data in tileData[x, y].HouseSprites)
        //                 {
        //                     if (data == null) continue;
        //                     // split _
        //                     string[] split = data.name.Split('_');
        //                     houseRenderers += split.Last() + ",";
        //                 }
        //                 GUI.Label(new Rect(screenPos.x - 20, screenPos.y - 50, 100, 20), houseRenderers);
        //             }
        //         }
        //     }
        // }
    }
}
