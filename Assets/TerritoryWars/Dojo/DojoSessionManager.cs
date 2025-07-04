using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.DataModels;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Models;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.SaveStorage;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using TerritoryWars.UI.Popups;
using UnityEngine;

namespace TerritoryWars.Dojo
{
    public class DojoSessionManager
    {
        private DojoGameManager _dojoGameManager;
        public bool IsGameWithBot { get; private set; }
        public bool IsGameWithBotAsPlayer { get; private set; }

        public static float TurnDuration = 65f;
        private GeneralAccount _localPlayerAccount => _dojoGameManager.LocalAccount;
        private evolute_duel_Board _localPlayerBoard;
        public int MoveCount => _dojoGameManager.WorldManager.Entities<evolute_duel_Move>().Length;

        private int _snapshotTurn = 0;
        private FieldElement _lastMoveId;
        private ContestProcessor _contestProcessor = new ContestProcessor();
        //public string last_move_id_hex { get; set; }
        //public int LastPlayerSide { get; set; }

        public delegate void MoveHandler(string playerAddress, TileData tile, Vector2Int position, int rotation, bool isJoker);

        public event MoveHandler OnMoveReceived;

        public delegate void SkipMoveHandler(string address);

        public event SkipMoveHandler OnSkipMoveReceived;

        public DojoSessionManager(DojoGameManager dojoGameManager)
        {
            _dojoGameManager = dojoGameManager;
            IsGameWithBot = SimpleStorage.LoadIsGameWithBot();
            IsGameWithBotAsPlayer = DojoGameManager.Instance.LocalBotAsPlayer != null;
            dojoGameManager.WorldManager.synchronizationMaster.OnModelUpdated.AddListener(OnModelUpdated);
            //dojoGameManager.WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
        }

        private void OnModelUpdated(ModelInstance modelInstance)
        {
            if (_dojoGameManager.IsTargetModel(modelInstance, nameof(evolute_duel_Board)))
            {
                //CustomLogger.LogImportant($"Model {nameof(evolute_duel_Board)} via OnModelUpdated");
            }
        }
        
        private void RoadContestWon(evolute_duel_RoadContestWon eventModel)
        {
            // string board_id = eventModel.board_id.Hex();
            //
            // if (LocalPlayerBoard.id.Hex() != board_id) return;
            //
            // byte root = eventModel.root;
            // int winner = eventModel.winner switch
            // {
            //     PlayerSide.Blue => 0,
            //     PlayerSide.Red => 1,
            // };
            // ushort red_points = eventModel.red_points;
            // ushort blue_points = eventModel.blue_points;
            //
            //
            // _contestProcessor.AddModel(new ContestInformation(root, ContestType.Road,() => 
            // {
            //     ContestAnimation(root, new ushort[] { blue_points, red_points }, () => UpdateBoardAfterRoadContest(root), true,
            //     false);
            // }));
            //
            // CustomLogger.LogExecution(
            //     $"[RoadContestWon] | Player: {winner} | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private void RoadContestDraw(evolute_duel_RoadContestDraw eventModel)
        {
            // string board_id = eventModel.board_id.Hex();
            //
            // if (LocalPlayerBoard.id.Hex() != board_id) return;
            //
            // byte root = eventModel.root;
            // ushort red_points = eventModel.red_points;
            // ushort blue_points = eventModel.blue_points;
            //
            // _contestProcessor.AddModel(new ContestInformation(root, ContestType.Road, () =>
            // {
            //     ContestAnimation(root, new ushort[] { blue_points, red_points }, () => UpdateBoardAfterRoadContest(root), true,
            //         false);
            // }));
            //
            // CustomLogger.LogExecution(
            //     $"[RoadContestDraw] | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }


        private void CityContestWon(evolute_duel_CityContestWon eventModel)
        {
            // string board_id = eventModel.board_id.Hex();
            //
            // if (LocalPlayerBoard.id.Hex() != board_id) return;
            //
            // byte root = eventModel.root;
            // int winner = eventModel.winner switch
            // {
            //     PlayerSide.Blue => 0,
            //     PlayerSide.Red => 1,
            // };
            // ushort red_points = eventModel.red_points;
            // ushort blue_points = eventModel.blue_points;
            //
            // _contestProcessor.AddModel(new ContestInformation(root, ContestType.City,() =>
            // {
            //     ContestAnimation(root, new ushort[] { blue_points, red_points },() =>  UpdateBoardAfterCityContest(root), false,
            //         true);
            // }));
            //
            // CustomLogger.LogExecution(
            //     $"[CityContestWon] | Player: {winner} | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private void CityContestDraw(evolute_duel_CityContestDraw eventModel)
        {
            // string board_id = eventModel.board_id.Hex();
            //
            // if (LocalPlayerBoard.id.Hex() != board_id) return;
            //
            // byte root = eventModel.root;
            // ushort red_points = eventModel.red_points;
            // ushort blue_points = eventModel.blue_points;
            //
            // _contestProcessor.AddModel(new ContestInformation(root, ContestType.City,() =>
            // {
            //     ContestAnimation(root, new ushort[] { blue_points, red_points }, () => UpdateBoardAfterCityContest(root), false,
            //         true);
            // }));
            //
            // CustomLogger.LogExecution(
            //     $"[CityContestDraw] | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private ClashAnimation CreateContestAnimation()
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            GameObject contestAnimationGO = PrefabsManager.Instance.InstantiateObject(PrefabsManager.Instance.ClashAnimationPrefab);
            ClashAnimation contestAnimation = contestAnimationGO.GetComponent<ClashAnimation>();
            return contestAnimation;
        }

        // private Dictionary<evolute_duel_CityNode, List<evolute_duel_CityNode>> cities;
        // private Dictionary<evolute_duel_RoadNode, List<evolute_duel_RoadNode>> roads;
        //
        // private void BuildCitySets()
        // {
        //     cities = new Dictionary<evolute_duel_CityNode, List<evolute_duel_CityNode>>();
        //     var cityNodesList = GetCityNodes();
        //     foreach (var cityNode in cityNodesList)
        //     {
        //         var root = GetCityRoot(cityNode);
        //         if (!cities.ContainsKey(root))
        //         {
        //             cities[root] = new List<evolute_duel_CityNode>();
        //         }
        //
        //         cities[root].Add(cityNode);
        //     }
        // }
        //
        // public KeyValuePair<T, List<T>> GetNearSetByPositionAndSide<T>(Vector2Int position, Side side)
        //     where T : class
        // {
        //     (Vector2Int targetPosition, Side targetSide) = GetNearTileSide(position, side);
        //     var root = OnChainBoardDataConverter.GetRootByPositionAndSide(targetPosition, targetSide);
        //     
        //     Dictionary<T, List<T>> targetDict;
        //     if (typeof(T) == typeof(evolute_duel_CityNode))
        //     {
        //         // update cities
        //         BuildCitySets();
        //         targetDict = cities as Dictionary<T, List<T>>;
        //     }
        //     else if (typeof(T) == typeof(evolute_duel_RoadNode))
        //     {
        //         // update roads
        //         BuildRoadSets();
        //         targetDict = roads as Dictionary<T, List<T>>;
        //     }
        //     else
        //     {
        //         return new KeyValuePair<T, List<T>>();
        //     }
        //
        //     foreach (var set in targetDict)
        //     {
        //         foreach (var node in set.Value)
        //         {
        //             INode iNode = node as INode;
        //             if (iNode == null) continue;
        //             byte nodePosition = iNode.GetPosition();
        //             if (nodePosition == root)
        //             {
        //                 return set;
        //             }
        //         }
        //     }
        //
        //     return new KeyValuePair<T, List<T>>();
        // }
        //
        // public KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>> GetCityByPosition(Vector2Int position)
        // {
        //     byte[] roots = OnChainBoardDataConverter.GetRootsByPosition(position);
        //     BuildCitySets();
        //     foreach (var root in roots)
        //     {
        //         foreach (var set in cities)
        //         {
        //             foreach (var node in set.Value)
        //             {
        //                 INode iNode = node as INode;
        //                 if (iNode == null) continue;
        //                 byte nodePosition = iNode.GetPosition();
        //                 if (nodePosition == root)
        //                 {
        //                     return set;
        //                 }
        //             }
        //         }
        //     }
        //     return new KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>>();
        // }
        //
        // public KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>> GetCityByPosition(byte position)
        // {
        //     BuildCitySets();
        //     foreach (var set in cities)
        //     {
        //         foreach (var node in set.Value)
        //         {
        //             INode iNode = node as INode;
        //             if (iNode == null) continue;
        //             byte nodePosition = iNode.GetPosition();
        //             if (nodePosition == position)
        //             {
        //                 return set;
        //             }
        //         }
        //     }
        //     return new KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>>();
        // }
        //
        // public KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>> GetRoadByPosition(Vector2Int position)
        // {
        //     byte[] roots = OnChainBoardDataConverter.GetRootsByPosition(position);
        //     BuildRoadSets();
        //     foreach (var root in roots)
        //     {
        //         foreach (var set in roads)
        //         {
        //             foreach (var node in set.Value)
        //             {
        //                 INode iNode = node as INode;
        //                 if (iNode == null) continue;
        //                 byte nodePosition = iNode.GetPosition();
        //                 if (nodePosition == root)
        //                 {
        //                     return set;
        //                 }
        //             }
        //         }
        //     }
        //     return new KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>>();
        // }
        //
        // public KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>> GetRoadByPosition(byte position)
        // {
        //     BuildRoadSets();
        //     foreach (var set in roads)
        //     {
        //         foreach (var node in set.Value)
        //         {
        //             INode iNode = node as INode;
        //             if (iNode == null) continue;
        //             byte nodePosition = iNode.GetPosition();
        //             if (nodePosition == position)
        //             {
        //                 return set;
        //             }
        //         }
        //     }
        //     return new KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>>();
        // }

        


        // private void BuildRoadSets()
        // {
        //     roads = new Dictionary<evolute_duel_RoadNode, List<evolute_duel_RoadNode>>();
        //     var roadNodesList = GetRoadNodes();
        //     foreach (var roadNode in roadNodesList)
        //     {
        //         var root = GetRoadRoot(roadNode);
        //         if (!roads.ContainsKey(root))
        //         {
        //             roads[root] = new List<evolute_duel_RoadNode>();
        //         }
        //
        //         roads[root].Add(roadNode);
        //     }
        // }
        //
        // private evolute_duel_RoadNode GetRoadRoot(evolute_duel_RoadNode road)
        // {
        //     if (road.position == road.parent)
        //     {
        //         return road;
        //     }
        //
        //     var parentPosition = road.parent;
        //     foreach (var roadNode in roadNodes)
        //     {
        //         if (roadNode.position == parentPosition)
        //         {
        //             return GetRoadRoot(roadNode);
        //         }
        //     }
        //
        //     return road;
        // }
        //
        // private evolute_duel_CityNode GetCityRoot(evolute_duel_CityNode city)
        // {
        //     if (city.position == city.parent)
        //     {
        //         return city;
        //     }
        //
        //     var parentPosition = city.parent;
        //     foreach (var cityNode in cityNodes)
        //     {
        //         if (cityNode.position == parentPosition)
        //         {
        //             return GetCityRoot(cityNode);
        //         }
        //     }
        //
        //     return city;
        // }

        public void UpdateBoardAfterCityContest(byte root)
        {
            // BuildCitySets();
            //
            // var city = GetCityByPosition(root);
            // foreach (var node in city.Value)
            // {
            //     bool isContested = city.Key.contested;
            //     Vector2Int position = OnChainBoardDataConverter.GetPositionByRoot(node.position);
            //     if (SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
            //     { 
            //         continue;
            //     }
            //     GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y);
            //     TileData tileData = SessionManagerOld.Instance.Board.GetTileData(position.x, position.y);
            //     TileGenerator tileGenerator = tile.GetComponent<TileGenerator>();
            //     int playerOwner;
            //     if (city.Key.contested)
            //     { 
            //         if (city.Key.blue_points == city.Key.red_points)
            //         { 
            //             playerOwner = 3;
            //         }
            //         else
            //         {
            //             playerOwner = city.Key.blue_points > city.Key.red_points ? 0 : 1;
            //         }
            //     }
            //     else
            //     {
            //         playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
            //     }
            //     tileGenerator.RecolorHouses(playerOwner, isContested, (byte)tileData.Rotation);
            //
            //     if (isContested)
            //     {
            //         tileGenerator.ChangeEnvironmentForContest();
            //         tileGenerator.tileParts.SetActiveWoodenArcs(false);
            //         tileGenerator.tileParts.SetActiveWoodenBorderWall(false);
            //     }
            //         
            //     SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
            //         BoardManager.StructureType.City, isContested); 
            // }
        }


        public void UpdateBoardAfterRoadContest(byte root)
        {
            // BuildRoadSets();
            //
            // var road = GetRoadByPosition(root);
            //
            //     foreach (var node in road.Value)
            //     {
            //         bool isContest = road.Key.contested;
            //         (Vector2Int position, Side side) = OnChainBoardDataConverter.GetPositionAndSide(node.position);
            //         if(SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
            //         {
            //             continue;
            //         }
            //         CustomLogger.LogInfo($"Board: " + SessionManagerOld.Instance.Board);
            //         CustomLogger.LogInfo($"TileObject: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y));
            //         CustomLogger.LogInfo($"TileGenerator: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>());
            //         TileGenerator tileGenerator = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>();
            //         int playerOwner;
            //         if (road.Key.contested)
            //         {
            //             if (road.Key.blue_points == road.Key.red_points)
            //             {
            //                 playerOwner = 3;
            //             }
            //             else
            //             {
            //                 playerOwner = road.Key.blue_points > road.Key.red_points ? 0 : 1;
            //             }
            //         }
            //         else
            //         {
            //             playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
            //         }
            //         tileGenerator.RecolorPinOnSide(playerOwner, (int)side, isContest);
            //         if (isContest)
            //         {
            //             tileGenerator.CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite =
            //                 PrefabsManager.Instance.TileAssetsObject.GetContestedRoadByReference(tileGenerator
            //                     .CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite);
            //             
            //         }
            //         SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
            //             BoardManager.StructureType.Road, false, isContest);
            //     }
        }

        public void UpdateBoardAfterContests()
        {
            // BuildCitySets();
            //
            // foreach (var city in cities)
            // {
            //     foreach (var node in city.Value)
            //     {
            //         bool isContested = city.Key.contested;
            //         Vector2Int position = OnChainBoardDataConverter.GetPositionByRoot(node.position);
            //         if (SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
            //         { 
            //             continue;
            //         }
            //         GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y);
            //         TileData tileData = SessionManagerOld.Instance.Board.GetTileData(position.x, position.y);
            //         TileGenerator tileGenerator = tile.GetComponent<TileGenerator>();
            //         int playerOwner;
            //         if (city.Key.contested)
            //         { 
            //             if (city.Key.blue_points == city.Key.red_points)
            //             { 
            //                 playerOwner = 3;
            //             }
            //             else
            //             {
            //                 playerOwner = city.Key.blue_points > city.Key.red_points ? 0 : 1;
            //             }
            //         }
            //         else
            //         {
            //             playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
            //         }
            //         tileGenerator.RecolorHouses(playerOwner, isContested, (byte)tileData.Rotation);
            //
            //         if (isContested)
            //         {
            //             tileGenerator.ChangeEnvironmentForContest();
            //             tileGenerator.tileParts.SetActiveWoodenBorderWall(false);
            //             tileGenerator.tileParts.SetActiveWoodenArcs(false);
            //         }
            //         
            //         SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
            //             BoardManager.StructureType.City, isContested); 
            //     }
            // }
            //
            // BuildRoadSets();
            //
            // foreach (var road in roads)
            // {
            //     foreach (var node in road.Value)
            //     {
            //         bool isContest = road.Key.contested;
            //         (Vector2Int position, Side side) = OnChainBoardDataConverter.GetPositionAndSide(node.position);
            //         if(SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
            //         {
            //             continue;
            //         }
            //         CustomLogger.LogInfo($"Board: " + SessionManagerOld.Instance.Board);
            //         CustomLogger.LogInfo($"TileObject: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y));
            //         CustomLogger.LogInfo($"TileGenerator: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>());
            //         TileGenerator tileGenerator = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>();
            //         int playerOwner;
            //         if (road.Key.contested)
            //         {
            //             if (road.Key.blue_points == road.Key.red_points)
            //             {
            //                 playerOwner = 3;
            //             }
            //             else
            //             {
            //                 playerOwner = road.Key.blue_points > road.Key.red_points ? 0 : 1;
            //             }
            //         }
            //         else
            //         {
            //             playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
            //         }
            //         tileGenerator.RecolorPinOnSide(playerOwner, (int)side, isContest);
            //         if (isContest)
            //         {
            //             tileGenerator.CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite =
            //                 PrefabsManager.Instance.TileAssetsObject.GetContestedRoadByReference(tileGenerator
            //                     .CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite);
            //             
            //         }
            //         SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
            //             BoardManager.StructureType.Road, false, isContest);
            //     }
            // }
        }
        


        //private List<evolute_duel_CityNode> cityNodes;
        // private List<evolute_duel_CityNode> GetCityNodes()
        // {
        //     // cityNodes = new List<evolute_duel_CityNode>();
        //     // GameObject[] cityNodesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_CityNode>();
        //     // foreach (var cityNodeGO in cityNodesGO)
        //     // {
        //     //     if (cityNodeGO.TryGetComponent(out evolute_duel_CityNode cityNode))
        //     //     {
        //     //         if (cityNode.board_id.Hex() == LocalPlayerBoard.id.Hex())
        //     //             cityNodes.Add(cityNode);
        //     //     }
        //     // }
        //     return cityNodes;
        // }

        // private List<evolute_duel_RoadNode> roadNodes;
        // private List<evolute_duel_RoadNode> GetRoadNodes()
        // {
        //     // roadNodes = new List<evolute_duel_RoadNode>();
        //     // GameObject[] roadNodesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_RoadNode>();
        //     // foreach (var roadNodeGO in roadNodesGO)
        //     // {
        //     //     if (roadNodeGO.TryGetComponent(out evolute_duel_RoadNode roadNode))
        //     //     {
        //     //         if (roadNode.board_id.Hex() == LocalPlayerBoard.id.Hex())
        //     //             roadNodes.Add(roadNode);
        //     //     }
        //     // }
        //     // return roadNodes;
        //     return new List<evolute_duel_RoadNode>();
        // }
        

        public void MakeMove(TileData data, int x, int y, bool isJoker)
        {
            GeneralAccount account = _dojoGameManager.LocalAccount;
            var serverTypes = DojoConverter.MoveClientToServer(data, x, y, isJoker);
            DojoConnector.MakeMove(account, serverTypes.joker_tile, serverTypes.rotation, serverTypes.col, serverTypes.row);
            //SessionManagerOld.Instance.isPlayerMakeMove = true;
        }

        public void CreateSnapshot()
        {
            //DojoConnector.CreateSnapshot(_localPlayerAccount, LocalPlayerBoard.id, (byte)MoveCount);
        }

        public void SkipMove()
        {
            DojoConnector.SkipMove(_localPlayerAccount);
        }

        private evolute_duel_Move GetMoveModelById(FieldElement move_id)
        {
            if (move_id == null) return null;
            GameObject[] movesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Move>();
            foreach (var moveGO in movesGO)
            {
                if (moveGO.TryGetComponent(out evolute_duel_Move move))
                {
                    string moveId = move_id.Hex();
                    if (moveId == null) continue;
                    if (move.id.Hex() == moveId)
                    {
                        return move;
                    }
                }
            }
            return null;

        }
        
        public void OnDestroy()
        {
            _dojoGameManager.WorldManager.synchronizationMaster.OnModelUpdated.RemoveListener(OnModelUpdated);
            //_dojoGameManager.WorldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnEventMessage);
        }

    }
}