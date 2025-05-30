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
        public evolute_duel_Move LastMove
        {
            get
            {
                if(_lastMoveId != null) return GetMoveModelById(_lastMoveId);
                if (_lastMoveId == null && _localPlayerBoard != null) return GetMoveModelById(_localPlayerBoard.last_move_id.Unwrap());
                return null;
            }
        }
        
        private ulong _lastMoveTimestamp;
        public ulong LastMoveTimestamp
        {
            get
            {
                if (_lastMoveTimestamp != 0) return _lastMoveTimestamp;
                return LastMove != null ? LastMove.timestamp : 0;
            }
            set => _lastMoveTimestamp = value;
        }
        //public string last_move_id_hex { get; set; }
        //public int LastPlayerSide { get; set; }

        public delegate void MoveHandler(string playerAddress, TileData tile, Vector2Int position, int rotation, bool isJoker);

        public event MoveHandler OnMoveReceived;

        public delegate void SkipMoveHandler(string address);

        public event SkipMoveHandler OnSkipMoveReceived;

        public evolute_duel_Board LocalPlayerBoard
        {
            get
            {
                if (_localPlayerBoard == null)
                {
                    _localPlayerBoard = GetLocalPlayerBoard();
                }

                return _localPlayerBoard;
            }
            private set => _localPlayerBoard = value;
        }

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

        private void OnEventMessage(ModelInstance modelInstance)
        {
            if (ApplicationState.CurrentState != ApplicationStates.Session) return;
            switch (modelInstance)
            {
                case evolute_duel_InvalidMove invalidMove:
                    InvalidMove(invalidMove);
                    break;
                case evolute_duel_NotYourTurn notYourTurn:
                    NotYourTurn(notYourTurn);
                    break;
                case evolute_duel_Skiped skipped:
                    Skipped(skipped);
                    break;
                case evolute_duel_Moved moved:
                    Moved(moved);
                    break;
                case evolute_duel_BoardUpdated boardUpdated:
                    BoardUpdated(boardUpdated);
                    break;
                case evolute_duel_GameFinished gameFinished:
                    GameFinished(gameFinished.board_id, gameFinished.host_player);
                    break;
                case evolute_duel_GameIsAlreadyFinished gameIsAlreadyFinished:
                    GameFinished(gameIsAlreadyFinished.board_id, gameIsAlreadyFinished.player_id);
                    break;
                case evolute_duel_RoadContestWon roadContestWon:
                    RoadContestWon(roadContestWon);
                    break;
                case evolute_duel_RoadContestDraw roadContestDraw:
                    RoadContestDraw(roadContestDraw);
                    break;
                case evolute_duel_CityContestWon cityContestWon:
                    CityContestWon(cityContestWon);
                    break;
                case evolute_duel_CityContestDraw cityContestDraw:
                    CityContestDraw(cityContestDraw);
                    break;
                case evolute_duel_GameCanceled gameCanceled:
                    GameCanceled(gameCanceled);
                    break;
            }

            if (_dojoGameManager.IsTargetModel(modelInstance, nameof(evolute_duel_Moved)))
            {
            }
            else if (_dojoGameManager.IsTargetModel(modelInstance, nameof(evolute_duel_Moved)))
            {
            }
        }

        private void Moved(evolute_duel_Moved eventModel)
        {
            string player = eventModel.player.Hex();
            if (player != SessionManagerOld.Instance.LocalPlayer.PlayerId &&
                player != SessionManagerOld.Instance.RemotePlayer.PlayerId) return;
            string move_id = eventModel.move_id.Hex();
            LastMoveTimestamp = eventModel.timestamp;
            string prev_move_id = eventModel.prev_move_id switch
            {
                Option<FieldElement>.Some id => id.value.Hex(),
                Option<FieldElement>.None => null
            };
            
            int rotation = (eventModel.rotation + 3) % 4;
            Vector2Int position = new Vector2Int(eventModel.col, eventModel.row);
            bool isJoker = eventModel.is_joker;
            TileData tile = eventModel.tile is Option<byte>.Some some
                ? new TileData(new TileModel(OnChainBoardDataConverter.TileTypes[some.value], rotation, position))
                : null;
            string board_id = eventModel.board_id.Hex();

            CustomLogger.LogExecution(
                $"[Moved] | Player: {player} | MoveId: {move_id} | PrevMoveId: {prev_move_id} | Tile: {tile} | Rotation: {rotation} | Position: {position} | IsJoker: {isJoker} | BoardId: {board_id}");
            OnMoveReceived?.Invoke(player, tile, position, rotation, isJoker);
        }

        private void InvalidMove(evolute_duel_InvalidMove eventModel)
        {
            string move_id = eventModel.move_id.Hex();
            string player = eventModel.player.Hex();
            
            if(player != SessionManagerOld.Instance.LocalPlayer.PlayerId &&
                player != SessionManagerOld.Instance.RemotePlayer.PlayerId) return;
            
            PopupManager.Instance.ShowInvalidMovePopup();

            CustomLogger.LogError($"[InvalidMove] | Player: {player} | MoveId: {move_id}");
        }

        private void NotYourTurn(evolute_duel_NotYourTurn eventModel)
        {
            string player = eventModel.player_id.Hex();

            if (player != SessionManagerOld.Instance.LocalPlayer.PlayerId) { return; }
            
            if(!SessionManagerOld.Instance.IsLocalPlayerTurn) return;
            PopupManager.Instance.NotYourTurnPopup();
            
            CustomLogger.LogError($"[NotYourTurn] | Player: {player}");
        }

        private void CantFinishGame(evolute_duel_CantFinishGame eventModel)
        {
            string player = eventModel.player_id.Hex();
            string board = eventModel.board_id.Hex();

            if (player != SessionManagerOld.Instance.LocalPlayer.PlayerId) { return; }
            
            PopupManager.Instance.ShowCantFinishGamePopup();
            
            CustomLogger.LogError($"[CantFinishGame] | Player: {player} | BoardId: {board}");
        }

        private void Skipped(evolute_duel_Skiped eventModel)
        {
            string player = eventModel.player.Hex();
            if(LastMoveTimestamp == eventModel.timestamp) return;
            LastMoveTimestamp = eventModel.timestamp;
            CustomLogger.LogExecution($"[Skipped] | Player: {player}");
            OnSkipMoveReceived?.Invoke(player);
        }

        public void LocalSkipped(string playerAddress)
        {
            // unix timestamp
            LastMoveTimestamp = (ulong) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            CustomLogger.LogExecution($"[LocalSkipped] | Player: {playerAddress}");
            OnSkipMoveReceived?.Invoke(playerAddress);
        }

        private void BoardUpdated(evolute_duel_BoardUpdated eventModel)
        {
            string board_id = eventModel.board_id.Hex();
            if (LocalPlayerBoard.id.Hex() != board_id) return;
            CustomLogger.LogExecution($"[BoardUpdated] | BoardId: {board_id}");
            int cityScoreFirst = eventModel.blue_score.Item1;
            int cartScoreFirst = eventModel.blue_score.Item2;
            int cityScoreSecond = eventModel.red_score.Item1;
            int cartScoreSecond = eventModel.red_score.Item2;
            float tilePlacementDuration = 0.8f;
            GameUI.Instance.playerInfoUI.SetCityScores(cityScoreFirst, cityScoreSecond, delay: tilePlacementDuration);
            GameUI.Instance.playerInfoUI.SetRoadScores(cartScoreFirst, cartScoreSecond, delay: tilePlacementDuration);
            GameUI.Instance.playerInfoUI.SetPlayerScores(cityScoreFirst + cartScoreFirst, cityScoreSecond + cartScoreSecond, delay: tilePlacementDuration);
            var tileData = eventModel.top_tile switch
            {
                Option<byte>.Some topTile => new TileData(new TileModel(OnChainBoardDataConverter.GetTopTile(topTile))),
                Option<byte>.None => null
            };
            var availableTiles = eventModel.available_tiles_in_deck.Length;
            var hostPlayerJokers = eventModel.player1.Item3;
            var guestPlayerJokers = eventModel.player2.Item3;
            SessionManagerOld.Instance.SetNextTile(tileData);
            SessionManagerOld.Instance.SetTilesInDeck(availableTiles + 1);
            SessionManagerOld.Instance.JokerManagerOld.SetJokersCount(0, hostPlayerJokers);
            SessionManagerOld.Instance.JokerManagerOld.SetJokersCount(1, guestPlayerJokers);
        }

        private void GameFinished(FieldElement board_id, FieldElement hostPlayer)
        {
            evolute_duel_Board localBoard = GetLocalPlayerBoard();
            bool isCurrentBoard = localBoard != null && localBoard.id.Hex() == board_id.Hex();
            bool isHostPlayerInSession = SessionManagerOld.Instance.LocalPlayer.PlayerId == hostPlayer.Hex() ||
                                         SessionManagerOld.Instance.RemotePlayer.PlayerId == hostPlayer.Hex();
            if (isCurrentBoard || isHostPlayerInSession)
            {
                _contestProcessor.SetGameFinished(true);
                SessionManagerOld.Instance.StructureHoverManager.IsGameFinished = true;
                CustomLogger.LogExecution($"[GameFinished]");
                FinishGameContests.FinishGameAction = () =>
                {
                    Coroutines.StartRoutine(GameFinishedDelayed());
                };
            }
        }

        private IEnumerator GameFinishedDelayed()
        {
            yield return new WaitForSeconds(2f);
            SimpleStorage.ClearCurrentBoardId();
            GameUI.Instance.ShowResultPopUp();
        }
    
        public int GetTurnCount()
        {
            evolute_duel_Move lastMove = LastMove;
            if (lastMove == null) return 0;
            ulong timestamp = lastMove.timestamp;
            float turnDuration = TurnDuration;
            
            ulong currentTime = (ulong)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            ulong timeDiff = currentTime - timestamp;
            int turnCount = (int)(timeDiff / turnDuration);
            return turnCount;
        }
        public int WhoseMove()
        {
            evolute_duel_Move lastMove = LastMove;
            if (lastMove == null) return 0;
            int side = lastMove.player_side.Unwrap();
            int turnCount = GetTurnCount();
            int playerSide = side;
            if (turnCount % 2 == 0)
            {
                return playerSide == 0 ? 1 : 0;
            }
            else
            {
                return playerSide;
            }
        }


        private void RoadContestWon(evolute_duel_RoadContestWon eventModel)
        {
            string board_id = eventModel.board_id.Hex();

            if (LocalPlayerBoard.id.Hex() != board_id) return;

            byte root = eventModel.root;
            int winner = eventModel.winner switch
            {
                PlayerSide.Blue => 0,
                PlayerSide.Red => 1,
            };
            ushort red_points = eventModel.red_points;
            ushort blue_points = eventModel.blue_points;
            
            
            _contestProcessor.AddModel(new ContestInformation(root, ContestType.Road,() => 
            {
                ContestAnimation(root, new ushort[] { blue_points, red_points }, () => UpdateBoardAfterRoadContest(root), true,
                false);
            }));
            
            CustomLogger.LogExecution(
                $"[RoadContestWon] | Player: {winner} | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private void RoadContestDraw(evolute_duel_RoadContestDraw eventModel)
        {
            string board_id = eventModel.board_id.Hex();

            if (LocalPlayerBoard.id.Hex() != board_id) return;

            byte root = eventModel.root;
            ushort red_points = eventModel.red_points;
            ushort blue_points = eventModel.blue_points;

            _contestProcessor.AddModel(new ContestInformation(root, ContestType.Road, () =>
            {
                ContestAnimation(root, new ushort[] { blue_points, red_points }, () => UpdateBoardAfterRoadContest(root), true,
                    false);
            }));

            CustomLogger.LogExecution(
                $"[RoadContestDraw] | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }


        private void CityContestWon(evolute_duel_CityContestWon eventModel)
        {
            string board_id = eventModel.board_id.Hex();

            if (LocalPlayerBoard.id.Hex() != board_id) return;

            byte root = eventModel.root;
            int winner = eventModel.winner switch
            {
                PlayerSide.Blue => 0,
                PlayerSide.Red => 1,
            };
            ushort red_points = eventModel.red_points;
            ushort blue_points = eventModel.blue_points;

            _contestProcessor.AddModel(new ContestInformation(root, ContestType.City,() =>
            {
                ContestAnimation(root, new ushort[] { blue_points, red_points },() =>  UpdateBoardAfterCityContest(root), false,
                    true);
            }));
            
            CustomLogger.LogExecution(
                $"[CityContestWon] | Player: {winner} | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private void CityContestDraw(evolute_duel_CityContestDraw eventModel)
        {
            string board_id = eventModel.board_id.Hex();

            if (LocalPlayerBoard.id.Hex() != board_id) return;

            byte root = eventModel.root;
            ushort red_points = eventModel.red_points;
            ushort blue_points = eventModel.blue_points;

            _contestProcessor.AddModel(new ContestInformation(root, ContestType.City,() =>
            {
                ContestAnimation(root, new ushort[] { blue_points, red_points }, () => UpdateBoardAfterCityContest(root), false,
                    true);
            }));

            CustomLogger.LogExecution(
                $"[CityContestDraw] | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private void GameCanceled(evolute_duel_GameCanceled eventModel)
        {
            string hostPlayer = eventModel.host_player.Hex();
            if (hostPlayer != SessionManagerOld.Instance.LocalPlayer.PlayerId &&
               hostPlayer != SessionManagerOld.Instance.RemotePlayer.PlayerId) return;

            SimpleStorage.ClearCurrentBoardId();
            PopupManager.Instance.ShowOpponentCancelGame();
        }

        private ClashAnimation CreateContestAnimation()
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            GameObject contestAnimationGO = PrefabsManager.Instance.InstantiateObject(PrefabsManager.Instance.ClashAnimationPrefab);
            ClashAnimation contestAnimation = contestAnimationGO.GetComponent<ClashAnimation>();
            return contestAnimation;
        }
        private void ContestAnimation(byte root, ushort[] points, Action recoloring, bool isRoadContest = false, bool isCityContest = false)
        {
            ClashAnimation contestAnimation = CreateContestAnimation();
            Vector2Int coord = OnChainBoardDataConverter.GetPositionByRoot(root);
            GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(coord.x, coord.y);
            if (tile)
            {
                Vector3 position = tile.transform.position;
                Vector3 offset = new Vector3(0, 1.5f, 0);
                int winner;
                if (points[0] > points[1])
                    winner = 0;
                else if (points[0] < points[1])
                    winner = 1;
                else
                    winner = -1;
                contestAnimation.Initialize(position + offset, winner, points, recoloring, isRoadContest, isCityContest);
            }
            else
            {
                Coroutines.StartRoutine(RemoteContestAnimation(coord, points, contestAnimation, recoloring, isRoadContest, isCityContest));
            }
        }

        private Dictionary<evolute_duel_CityNode, List<evolute_duel_CityNode>> cities;
        private Dictionary<evolute_duel_RoadNode, List<evolute_duel_RoadNode>> roads;

        private IEnumerator RemoteContestAnimation(Vector2Int coord, ushort[] points, ClashAnimation contestAnimation, Action recoloring, bool isRoadContest = false, bool isCityContest = false)
        {
            int i = 0;
            int maxAttempts = 6;
            while (i < maxAttempts)
            {
                GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(coord.x, coord.y);
                if (tile)
                {
                    Vector3 position = tile.transform.position;
                    Vector3 offset = new Vector3(0, 1.5f, 0);
                    int winner;
                    if (points[0] > points[1])
                        winner = 0;
                    else if (points[0] < points[1])
                        winner = 1;
                    else
                        winner = -1;
                    contestAnimation.Initialize(position + offset, winner, points, recoloring, isRoadContest, isCityContest);
                    break;

                }
                i++;
                yield return new WaitForSeconds(0.5f);
            }


        }

        private void BuildCitySets()
        {
            cities = new Dictionary<evolute_duel_CityNode, List<evolute_duel_CityNode>>();
            var cityNodesList = GetCityNodes();
            foreach (var cityNode in cityNodesList)
            {
                var root = GetCityRoot(cityNode);
                if (!cities.ContainsKey(root))
                {
                    cities[root] = new List<evolute_duel_CityNode>();
                }

                cities[root].Add(cityNode);
            }
        }

        public KeyValuePair<T, List<T>> GetNearSetByPositionAndSide<T>(Vector2Int position, Side side)
            where T : class
        {
            (Vector2Int targetPosition, Side targetSide) = GetNearTileSide(position, side);
            var root = OnChainBoardDataConverter.GetRootByPositionAndSide(targetPosition, targetSide);
            
            Dictionary<T, List<T>> targetDict;
            if (typeof(T) == typeof(evolute_duel_CityNode))
            {
                // update cities
                BuildCitySets();
                targetDict = cities as Dictionary<T, List<T>>;
            }
            else if (typeof(T) == typeof(evolute_duel_RoadNode))
            {
                // update roads
                BuildRoadSets();
                targetDict = roads as Dictionary<T, List<T>>;
            }
            else
            {
                return new KeyValuePair<T, List<T>>();
            }
            string dictContent = "";
            foreach (var set in targetDict)
            {
                dictContent += "Root: " + set.Key + " | ";
                foreach (var node in set.Value)
                {
                    INode iNode = node as INode;
                    dictContent += $"\n {node} | {iNode.GetPosition()}";
                }
            }

            foreach (var set in targetDict)
            {
                foreach (var node in set.Value)
                {
                    INode iNode = node as INode;
                    if (iNode == null) continue;
                    byte nodePosition = iNode.GetPosition();
                    if (nodePosition == root)
                    {
                        return set;
                    }
                }
            }

            return new KeyValuePair<T, List<T>>();
        }

        public KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>> GetCityByPosition(Vector2Int position)
        {
            byte[] roots = OnChainBoardDataConverter.GetRootsByPosition(position);
            BuildCitySets();
            foreach (var root in roots)
            {
                foreach (var set in cities)
                {
                    foreach (var node in set.Value)
                    {
                        INode iNode = node as INode;
                        if (iNode == null) continue;
                        byte nodePosition = iNode.GetPosition();
                        if (nodePosition == root)
                        {
                            return set;
                        }
                    }
                }
            }
            return new KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>>();
        }
        
        public KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>> GetCityByPosition(byte position)
        {
            BuildCitySets();
            foreach (var set in cities)
            {
                foreach (var node in set.Value)
                {
                    INode iNode = node as INode;
                    if (iNode == null) continue;
                    byte nodePosition = iNode.GetPosition();
                    if (nodePosition == position)
                    {
                        return set;
                    }
                }
            }
            return new KeyValuePair<evolute_duel_CityNode, List<evolute_duel_CityNode>>();
        }
        
        public KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>> GetRoadByPosition(Vector2Int position)
        {
            byte[] roots = OnChainBoardDataConverter.GetRootsByPosition(position);
            BuildRoadSets();
            foreach (var root in roots)
            {
                foreach (var set in roads)
                {
                    foreach (var node in set.Value)
                    {
                        INode iNode = node as INode;
                        if (iNode == null) continue;
                        byte nodePosition = iNode.GetPosition();
                        if (nodePosition == root)
                        {
                            return set;
                        }
                    }
                }
            }
            return new KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>>();
        }
        
        public KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>> GetRoadByPosition(byte position)
        {
            BuildRoadSets();
            foreach (var set in roads)
            {
                foreach (var node in set.Value)
                {
                    INode iNode = node as INode;
                    if (iNode == null) continue;
                    byte nodePosition = iNode.GetPosition();
                    if (nodePosition == position)
                    {
                        return set;
                    }
                }
            }
            return new KeyValuePair<evolute_duel_RoadNode, List<evolute_duel_RoadNode>>();
        }

        private (Vector2Int, Side) GetNearTileSide(Vector2Int position, Side side)
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


        private void BuildRoadSets()
        {
            roads = new Dictionary<evolute_duel_RoadNode, List<evolute_duel_RoadNode>>();
            var roadNodesList = GetRoadNodes();
            foreach (var roadNode in roadNodesList)
            {
                var root = GetRoadRoot(roadNode);
                if (!roads.ContainsKey(root))
                {
                    roads[root] = new List<evolute_duel_RoadNode>();
                }

                roads[root].Add(roadNode);
            }
        }

        private evolute_duel_RoadNode GetRoadRoot(evolute_duel_RoadNode road)
        {
            if (road.position == road.parent)
            {
                return road;
            }

            var parentPosition = road.parent;
            foreach (var roadNode in roadNodes)
            {
                if (roadNode.position == parentPosition)
                {
                    return GetRoadRoot(roadNode);
                }
            }

            return road;
        }

        private evolute_duel_CityNode GetCityRoot(evolute_duel_CityNode city)
        {
            if (city.position == city.parent)
            {
                return city;
            }

            var parentPosition = city.parent;
            foreach (var cityNode in cityNodes)
            {
                if (cityNode.position == parentPosition)
                {
                    return GetCityRoot(cityNode);
                }
            }

            return city;
        }

        public void UpdateBoardAfterCityContest(byte root)
        {
            BuildCitySets();

            var city = GetCityByPosition(root);
            foreach (var node in city.Value)
            {
                bool isContested = city.Key.contested;
                Vector2Int position = OnChainBoardDataConverter.GetPositionByRoot(node.position);
                if (SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
                { 
                    continue;
                }
                GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y);
                TileData tileData = SessionManagerOld.Instance.Board.GetTileData(position.x, position.y);
                TileGenerator tileGenerator = tile.GetComponent<TileGenerator>();
                int playerOwner;
                if (city.Key.contested)
                { 
                    if (city.Key.blue_points == city.Key.red_points)
                    { 
                        playerOwner = 3;
                    }
                    else
                    {
                        playerOwner = city.Key.blue_points > city.Key.red_points ? 0 : 1;
                    }
                }
                else
                {
                    playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
                }
                tileGenerator.RecolorHouses(playerOwner, isContested, (byte)tileData.Rotation);

                if (isContested)
                {
                    tileGenerator.ChangeEnvironmentForContest();
                    tileGenerator.tileParts.SetActiveWoodenArcs(false);
                    tileGenerator.tileParts.SetActiveWoodenBorderWall(false);
                }
                    
                SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
                    BoardManager.StructureType.City, isContested); 
            }
        }


        public void UpdateBoardAfterRoadContest(byte root)
        {
            BuildRoadSets();
            
            var road = GetRoadByPosition(root);
            
                foreach (var node in road.Value)
                {
                    bool isContest = road.Key.contested;
                    (Vector2Int position, Side side) = OnChainBoardDataConverter.GetPositionAndSide(node.position);
                    if(SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
                    {
                        continue;
                    }
                    CustomLogger.LogInfo($"Board: " + SessionManagerOld.Instance.Board);
                    CustomLogger.LogInfo($"TileObject: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y));
                    CustomLogger.LogInfo($"TileGenerator: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>());
                    TileGenerator tileGenerator = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>();
                    int playerOwner;
                    if (road.Key.contested)
                    {
                        if (road.Key.blue_points == road.Key.red_points)
                        {
                            playerOwner = 3;
                        }
                        else
                        {
                            playerOwner = road.Key.blue_points > road.Key.red_points ? 0 : 1;
                        }
                    }
                    else
                    {
                        playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
                    }
                    tileGenerator.RecolorPinOnSide(playerOwner, (int)side, isContest);
                    if (isContest)
                    {
                        tileGenerator.CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite =
                            PrefabsManager.Instance.TileAssetsObject.GetContestedRoadByReference(tileGenerator
                                .CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite);
                        
                    }
                    SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
                        BoardManager.StructureType.Road, false, isContest);
                }
        }

        public void UpdateBoardAfterContests()
        {
            BuildCitySets();

            foreach (var city in cities)
            {
                foreach (var node in city.Value)
                {
                    bool isContested = city.Key.contested;
                    Vector2Int position = OnChainBoardDataConverter.GetPositionByRoot(node.position);
                    if (SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
                    { 
                        continue;
                    }
                    GameObject tile = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y);
                    TileData tileData = SessionManagerOld.Instance.Board.GetTileData(position.x, position.y);
                    TileGenerator tileGenerator = tile.GetComponent<TileGenerator>();
                    int playerOwner;
                    if (city.Key.contested)
                    { 
                        if (city.Key.blue_points == city.Key.red_points)
                        { 
                            playerOwner = 3;
                        }
                        else
                        {
                            playerOwner = city.Key.blue_points > city.Key.red_points ? 0 : 1;
                        }
                    }
                    else
                    {
                        playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
                    }
                    tileGenerator.RecolorHouses(playerOwner, isContested, (byte)tileData.Rotation);

                    if (isContested)
                    {
                        tileGenerator.ChangeEnvironmentForContest();
                        tileGenerator.tileParts.SetActiveWoodenBorderWall(false);
                        tileGenerator.tileParts.SetActiveWoodenArcs(false);
                    }
                    
                    SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
                        BoardManager.StructureType.City, isContested); 
                }
            }
            
            BuildRoadSets();

            foreach (var road in roads)
            {
                foreach (var node in road.Value)
                {
                    bool isContest = road.Key.contested;
                    (Vector2Int position, Side side) = OnChainBoardDataConverter.GetPositionAndSide(node.position);
                    if(SessionManagerOld.Instance.Board == null || SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y) == null)
                    {
                        continue;
                    }
                    CustomLogger.LogInfo($"Board: " + SessionManagerOld.Instance.Board);
                    CustomLogger.LogInfo($"TileObject: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y));
                    CustomLogger.LogInfo($"TileGenerator: " + SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>());
                    TileGenerator tileGenerator = SessionManagerOld.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>();
                    int playerOwner;
                    if (road.Key.contested)
                    {
                        if (road.Key.blue_points == road.Key.red_points)
                        {
                            playerOwner = 3;
                        }
                        else
                        {
                            playerOwner = road.Key.blue_points > road.Key.red_points ? 0 : 1;
                        }
                    }
                    else
                    {
                        playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
                    }
                    tileGenerator.RecolorPinOnSide(playerOwner, (int)side, isContest);
                    if (isContest)
                    {
                        tileGenerator.CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite =
                            PrefabsManager.Instance.TileAssetsObject.GetContestedRoadByReference(tileGenerator
                                .CurrentTileGO.GetComponent<TileParts>().RoadRenderers[(int)side].sprite);
                        
                    }
                    SessionManagerOld.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y,
                        BoardManager.StructureType.Road, false, isContest);
                }
            }
        }
        
        public void CloseAllStructure()
        {
            SessionManagerOld.Instance.Board.CloseAllStructures();
        }


        private List<evolute_duel_CityNode> cityNodes;
        private List<evolute_duel_CityNode> GetCityNodes()
        {
            cityNodes = new List<evolute_duel_CityNode>();
            GameObject[] cityNodesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_CityNode>();
            foreach (var cityNodeGO in cityNodesGO)
            {
                if (cityNodeGO.TryGetComponent(out evolute_duel_CityNode cityNode))
                {
                    if (cityNode.board_id.Hex() == LocalPlayerBoard.id.Hex())
                        cityNodes.Add(cityNode);
                }
            }
            return cityNodes;
        }

        private List<evolute_duel_RoadNode> roadNodes;
        private List<evolute_duel_RoadNode> GetRoadNodes()
        {
            roadNodes = new List<evolute_duel_RoadNode>();
            GameObject[] roadNodesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_RoadNode>();
            foreach (var roadNodeGO in roadNodesGO)
            {
                if (roadNodeGO.TryGetComponent(out evolute_duel_RoadNode roadNode))
                {
                    if (roadNode.board_id.Hex() == LocalPlayerBoard.id.Hex())
                        roadNodes.Add(roadNode);
                }
            }
            return roadNodes;
        }

        public evolute_duel_Board GetLocalPlayerBoard()
        {
            return GetBoard(_dojoGameManager.LocalAccount.Address.Hex());
        }

        public evolute_duel_Board GetBoard(string playerAddress)
        {
            GameObject[] boardsGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Board>();
            foreach (var boardGO in boardsGO)
            {
                if (boardGO.TryGetComponent(out evolute_duel_Board board))
                {
                    //public (FieldElement, PlayerSide, byte, bool) player1;
                    if (board.player1.Item1.Hex() == playerAddress || board.player2.Item1.Hex() == playerAddress)
                    {
                        return board;
                    }
                }
            }
            return null;
        }


        public TileData GetTopTile()
        {
            if (LocalPlayerBoard == null) return null;
            return new TileData(new TileModel(OnChainBoardDataConverter.GetTopTile(LocalPlayerBoard.top_tile)));
        }

        public void MakeMove(TileData data, int x, int y, bool isJoker)
        {
            GeneralAccount account = _dojoGameManager.LocalAccount;
            var serverTypes = DojoConverter.MoveClientToServer(data, x, y, isJoker);
            DojoConnector.MakeMove(account, serverTypes.joker_tile, serverTypes.rotation, serverTypes.col, serverTypes.row);
            SessionManagerOld.Instance.isPlayerMakeMove = true;
        }

        public void CreateSnapshot()
        {
            DojoConnector.CreateSnapshot(_localPlayerAccount, LocalPlayerBoard.id, (byte)MoveCount);
        }

        public void SkipMove()
        {
            if (SessionManagerOld.Instance.isPlayerMakeMove) return;
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
            _dojoGameManager.WorldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnEventMessage);
        }

    }
}