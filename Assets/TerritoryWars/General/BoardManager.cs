using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using JetBrains.Annotations;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.Managers.SessionComponents;
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

    public class BoardManager : MonoBehaviour
    {
        public TileAssetsObject tileAssets => PrefabsManager.Instance.TileAssetsObject;

        [SerializeField] private CloudsController CloudsController;
        [SerializeField] private int width => GameConfiguration.ClientBoardSize.x;
        [SerializeField] private int height => GameConfiguration.ClientBoardSize.y;
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
        }

        private void InitializeBoard()
        {
            tileObjects = new GameObject[width, height];
            tileData = new TileData[width, height];
        }

        public void Initialize(Board board)
        {
            InitializeBoard();
            foreach (var tile in board.Tiles.Values)
            {
                if (IsEdgeTile(tile.Position.x, tile.Position.y)) PlaceEdgeTile(tile);
                else if (!tile.IsNull) PlaceTile(tile);
            }
        }

        public void PlaceEdgeTile(TileModel tileModel)
        {

            if (tileModel.Position == new Vector2Int(0, 9) || tileModel.Position == new Vector2Int(9, 0)) return;

            PlaceTile(tileModel);
            if (tileModel.Type == "FFFF")
            {
                SpawnMountain(tileModel);
            }

            if (tileModel.Type.Contains('R'))
            {
                SpawnRoad(tileModel);
            }

            if (tileModel.Type.Contains('C'))
            {
                SpawnCity(tileModel);
            }


        }

        private void SpawnMountain(TileModel tileModel)
        {
            GameObject mountain;
            if (IsSnowBoardPart(tileModel.Position))
            {
                mountain = Instantiate(PrefabsManager.Instance.GetRandomSnowMountainTile(),
                    tileObjects[tileModel.Position.x, tileModel.Position.y].transform.Find("RoadRenderer"));
            }
            else
            {
                mountain = Instantiate(PrefabsManager.Instance.GetRandomMountainTile(),
                    tileObjects[tileModel.Position.x, tileModel.Position.y].transform.Find("RoadRenderer"));
            }
            foreach (var mountainSprite in mountain.GetComponentsInChildren<SpriteRenderer>())
            {
                mountainSprite.sortingOrder = 14;
            }
            TileParts[] tileParts = tileObjects[tileModel.Position.x, tileModel.Position.y]
                .GetComponentsInChildren<TileParts>();
            tileParts.ToList().ForEach(
                tileParts =>
                {
                    tileParts.Enviroment.SetActive(false);
                    tileParts.HideForestAreas();
                    tileParts.Areas.ToList().ForEach(area =>
                    {
                        if (area != null) Destroy(area.gameObject);
                    });
                    tileParts.Areas.Clear();
                });
            CloudsController.AddMountain(mountain.transform.position);
        }

        private void SpawnRoad(TileModel tileModel)
        {
            GameObject tileObject = tileObjects[tileModel.Position.x, tileModel.Position.y];
            Transform arc = tileObject.transform.Find("BorderArc");
            arc.gameObject.SetActive(true);
            TileRotator.GetMirrorRotationStatic(arc, tileModel.Rotation);
            tileObject.GetComponentsInChildren<TileParts>().ToList().ForEach(tileParts =>
            {
                tileParts.Mill.SetActive(false);
                tileParts.Forest[tileModel.Rotation].SetActive(true);
            });
        }

        private void SpawnCity(TileModel tileModel)
        {
            tileObjects[tileModel.Position.x, tileModel.Position.y].GetComponentsInChildren<TileParts>().ToList().ForEach(tileParts =>
            {
                tileParts.Forest[tileModel.Rotation].SetActive(true);
            });
        }


        public bool PlaceTile(TileModel tile)
        {
            TileData data = new TileData(tile);
            return PlaceTile(data);
        }

        public bool PlaceTile(TileData tile)
        { 
            tileData[tile.Position.x, tile.Position.y] = tile;
            GameObject tileObject = Instantiate(tilePrefab, GetTilePosition(tile.Position.x, tile.Position.y), Quaternion.identity, transform);
            tileObject.name += $"_{tile.Position.x}_{tile.Position.y}";
            tileObject.GetComponent<TileGenerator>().Generate(tile, true);
            tileObjects[tile.Position.x, tile.Position.y] = tileObject;
            PlacedTiles[tile.Position] = tile;

            var (x, y) = (tile.Position.x, tile.Position.y);

            if ((x == 1 || x == width - 2 || y == 1 || y == height - 2) && !IsEdgeTile(x, y))
            {
                TryConnectEdgeStructure(tile.PlayerSide, x, y, StructureType.None, false, false, true);
                //if (isConnected)
                //{
                //    ConnectEdgeStructureAnimation(tile.PlayerSide, tile, x, y);
                //}
            }

            EventBus.Publish(new TilePlacedEvent(tile));
            return true;
        }

        public struct TilePlacedEvent
        {
            public TileData TileData;
            public TilePlacedEvent(TileData tile) => TileData = tile;
        }

        public void FloatingTextAnimation(TileData tile)
        {
            Vector2Int position = tile.Position;
            GameObject gameObject = GetTileObject(position.x, position.y);
            TileData tileData = GetTileData(position.x, position.y);
            TileParts tileParts = gameObject.GetComponentInChildren<TileParts>();
            TileGenerator tileGenerator = gameObject.GetComponent<TileGenerator>();
            int playerId = tile.PlayerSide;
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
                if (pin == null) continue;
                float duration = 2f + UnityEngine.Random.Range(0f, 1f);
                Vector3 textPosition = pin.transform.position;
                int cityCount = tileData.Type.Count(c => c == 'R');
                string messageText = cityCount == 2 ? "+2" : "+1";
                FloatingTextManager.Instance.Show(messageText, textPosition, motion, duration, "road_icon_" + side);
                if (cityCount == 2) break;
            }
        }

        public void ScoreClientPrediction(int playerIndex, TileData data)
        {
            if (data.PlayerSide != SessionManager.Instance.SessionContext.LocalPlayer.PlayerSide) return;
            GameUI.Instance.playerInfoUI.AddClientCityScore(playerIndex, data.Type.Count(c => c == 'C') * 2);
            GameUI.Instance.playerInfoUI.AddClientRoadScore(playerIndex, data.Type.Count(r => r == 'R'));
        }

        public bool RevertTile(int x, int y)
        {
            Destroy(tileObjects[x, y]);
            tileObjects[x, y] = null;
            tileData[x, y] = null;
            PlacedTiles.Remove(new Vector2Int(x, y));

            if (tileObjects[x, y] == null && tileData[x, y] == null && PlacedTiles.ContainsKey(new Vector2Int(x, y)))
            {
                return true;
            }

            return false;
        }

        public void CheckAndConnectEdgeStructure(int ownerId, int x, int y, StructureType type, bool isCityContest = false, bool isRoadContest = false)
        {
            if( (x == 1 || x == width - 2 || y == 1 || y == height - 2) && !IsEdgeTile(x, y))
            {
                TryConnectEdgeStructure(ownerId, x, y, type, isCityContest, isRoadContest, false);
            }
        }

        public void ConnectEdgeStructureAnimation(int ownerId, TileData tileData, int x, int y, bool isCityContest = false, bool isRoadContest = false, bool isPlacing = false)
        {
            if (isCityContest || isRoadContest || !isPlacing| !SessionManager.Instance.IsInitialized) return;
            FloatingTextAnimation(tileData);
            ScoreClientPrediction(ownerId, tileData);
        }

        private bool TryConnectEdgeStructure(int owner, int x, int y, StructureType type = StructureType.None, bool isCityContest = false, bool isRoadContest = false, bool isPlacing = false)
        {
            bool result = false;
            GameObject[] neighborsGO = new GameObject[4];
            TileData[] neighborsData = new TileData[4];
            int[][] positions = new[] { new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { -1, 0 }, new int[] { 0, 1 } };
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

            for (int i = 0; i < 4; i++)
            {
                if (IsEdgeTile(tilePositions[i][0], tilePositions[i][1]) && neighborsGO[i] != null)
                {
                    if (owner != 3) neighborsData[i].SetOwner(owner);
                    ConnectEdgeStructureAnimation(owner, neighborsData[i], tilePositions[i][0], tilePositions[i][1], isCityContest, isRoadContest, isPlacing);
                    TileGenerator tileGenerator = neighborsGO[i].GetComponent<TileGenerator>();
                    if (type == StructureType.None || type == StructureType.City)
                    {
                        tileGenerator.RecolorHouses(owner, isCityContest, neighborsData[i].Rotation, true);
                        result = true;
                    }

                    if (type == StructureType.None || type == StructureType.Road)
                    {
                        TileGenerator placedTileGenerator = tileObjects[x, y].GetComponent<TileGenerator>();
                        List<Side> roadSides = neighborsData[i].GetRoadSides();
                        if (roadSides.Count == 0) continue;
                        result = true;
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
            return result;
        }

        public bool IsSnowBoardPart(Vector2Int position)
        {
            if (position.x > 4 & position.y > 4)
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
        
        public List<Side> CheckCityTileSidesToBorder(int x, int y)
        {
            // returns list int of sides that are closer to the border 
            // 0 - TOP
            // 1 - Right
            // 2 - Bottom
            // 3 - Left

            List<Side> closerSides = new List<Side>();
            if (IsEdgeTile(x, y)) return closerSides;
            TileData tile = GetTileData(x + 1, y);
            if (IsEdgeTile(x + 1, y) && tile?.Type == "FFFF")
            {
                closerSides.Add(Side.Top);
            }

            tile = GetTileData(x, y - 1);
            if (IsEdgeTile(x, y - 1) && tile?.Type == "FFFF")
            {
                closerSides.Add(Side.Right);
            }

            tile = GetTileData(x - 1, y);
            if (IsEdgeTile(x - 1, y) && tile?.Type == "FFFF")
            {
                closerSides.Add(Side.Bottom);
            }

            tile = GetTileData(x, y + 1);
            if (IsEdgeTile(x, y + 1) && tile?.Type == "FFFF")
            {
                closerSides.Add(Side.Left);
            }

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
            if (IsEdgeTile(x, y)) return closerSides;

            if (GetTileData(x + 1, y) == null) { closerSides.Add(Side.Top); }

            if (GetTileData(x, y - 1) == null) { closerSides.Add(Side.Right); }

            if (GetTileData(x - 1, y) == null) { closerSides.Add(Side.Bottom); }

            if (GetTileData(x, y + 1) == null) { closerSides.Add(Side.Left); }

            return closerSides;
        }

        public List<MineTileInfo> CheckRoadTileSidesToBorder(int x, int y)
        {
            // returns list int of sides that are closer to the border 
            // 0 - TOP
            // 1 - Right
            // 2 - Bottom
            // 3 - Left

            List<MineTileInfo> closerSides = new List<MineTileInfo>();
            if (IsEdgeTile(x, y)) return closerSides;
            TileData tile = GetTileData(x + 1, y);
            if (IsEdgeTile(x + 1, y) && tile != null && !tile.IsRoad())
            {
                closerSides.Add(new MineTileInfo
                {
                    Tile = GetTileObject(x + 1, y),
                    Position = GetTilePosition(x + 1, y),
                    TileBoardPosition = new Vector2Int(x + 1, y),
                    Direction = Side.Top
                });
            }

            tile = GetTileData(x, y - 1);
            if (IsEdgeTile(x, y - 1) && tile != null && !tile.IsRoad())
            {
                closerSides.Add(new MineTileInfo
                {
                    Tile = GetTileObject(x, y - 1),
                    Position = GetTilePosition(x, y - 1),
                    TileBoardPosition = new Vector2Int(x, y - 1),
                    Direction = Side.Right
                });
            }

            tile = GetTileData(x - 1, y);
            if (IsEdgeTile(x - 1, y) && tile != null && !tile.IsRoad())
            {
                closerSides.Add(new MineTileInfo
                {
                    Tile = GetTileObject(x - 1, y),
                    Position = GetTilePosition(x - 1, y),
                    TileBoardPosition = new Vector2Int(x - 1, y),
                    Direction = Side.Bottom
                });
            }

            tile = GetTileData(x, y + 1);
            if (IsEdgeTile(x, y + 1) && tile != null && !tile.IsRoad())
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


            if (placedTiles < 33)
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
        
        public static (Vector2Int, Side) GetNearTileSide(Vector2Int position, Side side)
        {
            Vector2Int targetPosition = position;
            Side targetSide = side;
            switch (side)
            {
                case Side.Top:
                    targetPosition = new Vector2Int(position.x + 1, position.y);
                    targetSide = Side.Bottom;
                    break;
                case Side.Right:
                    targetPosition = new Vector2Int(position.x, position.y - 1);
                    targetSide = Side.Left;
                    break;
                case Side.Bottom:
                    targetPosition = new Vector2Int(position.x - 1, position.y);
                    targetSide = Side.Top;
                    break;
                case Side.Left:
                    targetPosition = new Vector2Int(position.x, position.y + 1);
                    targetSide = Side.Right;
                    break;
            }
            return (targetPosition, targetSide);
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

                    tile.Rotate(4 - (tile.Rotation % 4));
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


            int initialRotation = tile.Rotation;


            for (int rotation = 0; rotation < 4; rotation++)
            {
                if (CanPlaceTile(tile, x, y))
                {
                    validRotations.Add(rotation);
                }
                tile.Rotate();
            }


            tile.Rotate(4 - (tile.Rotation % 4));
            while (tile.Rotation != initialRotation)
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
