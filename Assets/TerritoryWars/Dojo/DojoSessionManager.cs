using System;
using System.Collections;
using System.Collections.Generic;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars.Dojo
{
    public class DojoSessionManager
    {
        private DojoGameManager _dojoGameManager;

        private Account _localPlayerAccount => _dojoGameManager.LocalBurnerAccount;
        private evolute_duel_Board _localPlayerBoard;
        private int _moveCount = 0;

        private int _snapshotTurn = 0;
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
            dojoGameManager.WorldManager.synchronizationMaster.OnModelUpdated.AddListener(OnModelUpdated);
            dojoGameManager.WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
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
            if(ApplicationState.CurrentState != ApplicationStates.Session) return;
            switch (modelInstance)
            {
                case evolute_duel_Moved moved:
                    Moved(moved);
                    break;
                case evolute_duel_InvalidMove invalidMove:
                    InvalidMove(invalidMove);
                    break;
                case evolute_duel_Skiped skipped:
                    Skipped(skipped);
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
            if (player != SessionManager.Instance.LocalPlayer.Address.Hex() &&
                player != SessionManager.Instance.RemotePlayer.Address.Hex()) return;

            _moveCount++;
            string move_id = eventModel.move_id.Hex();
            string prev_move_id = eventModel.prev_move_id switch
            {
                Option<FieldElement>.Some id => id.value.Hex(),
                Option<FieldElement>.None => null
            };
            TileData tile = eventModel.tile is Option<byte>.Some some
                ? new TileData(OnChainBoardDataConverter.TileTypes[some.value])
                : null;
            int rotation = (eventModel.rotation + 3) % 4;
            Vector2Int position = new Vector2Int(eventModel.col, eventModel.row);
            bool isJoker = eventModel.is_joker;
            string board_id = eventModel.board_id.Hex();

            CustomLogger.LogEvent(
                $"[Moved] | Player: {player} | MoveId: {move_id} | PrevMoveId: {prev_move_id} | Tile: {tile} | Rotation: {rotation} | Position: {position} | IsJoker: {isJoker} | BoardId: {board_id}");
            OnMoveReceived?.Invoke(player, tile, position, rotation, isJoker);
        }

        private void InvalidMove(evolute_duel_InvalidMove eventModel)
        {
            string move_id = eventModel.move_id.Hex();
            string player = eventModel.player.Hex();

            CustomLogger.LogError($"[InvalidMove] | Player: {player} | MoveId: {move_id}");
        }

        private void Skipped(evolute_duel_Skiped eventModel)
        {
            string player = eventModel.player.Hex();
            CustomLogger.LogEvent($"[Skipped] | Player: {player}");
            OnSkipMoveReceived?.Invoke(player);
        }

        private void BoardUpdated(evolute_duel_BoardUpdated eventModel)
        {
            string board_id = eventModel.board_id.Hex();
            if (LocalPlayerBoard.id.Hex() != board_id) return;
            CustomLogger.LogEvent($"[BoardUpdated] | BoardId: {board_id}");
            int cityScoreBlue = eventModel.blue_score.Item1;
            int cartScoreBlue = eventModel.blue_score.Item2;
            int cityScoreRed = eventModel.red_score.Item1;
            int cartScoreRed = eventModel.red_score.Item2;
            GameUI.Instance.SessionUI.SetCityScores(cityScoreBlue, cityScoreRed);
            GameUI.Instance.SessionUI.SetRoadScores(cartScoreBlue, cartScoreRed);
            GameUI.Instance.SessionUI.SetPlayerScores(cityScoreBlue + cartScoreBlue, cityScoreRed + cartScoreRed);
            var tileData = eventModel.top_tile switch
            {
                Option<byte>.Some topTile => new TileData(OnChainBoardDataConverter.GetTopTile(topTile)),
                Option<byte>.None => null
            };
            var availableTiles = eventModel.available_tiles_in_deck.Length;
            var hostPlayerJokers = eventModel.player1.Item3;
            var guestPlayerJokers = eventModel.player2.Item3;
            CustomLogger.LogImportant($"BoardUpdated. TopTile: {tileData.id}");
            SessionManager.Instance.SetNextTile(tileData);
            SessionManager.Instance.SetTilesInDeck(availableTiles + 1);
            SessionManager.Instance.JokerManager.SetJokersCount(0, hostPlayerJokers);
            SessionManager.Instance.JokerManager.SetJokersCount(1, guestPlayerJokers);
        }

        private void GameFinished(FieldElement board_id, FieldElement hostPlayer)
        {
            evolute_duel_Board localBoard = GetLocalPlayerBoard();
            bool isCurrentBoard = localBoard != null && localBoard.id.Hex() == board_id.Hex();
            bool isHostPlayerInSession = SessionManager.Instance.LocalPlayer.Address.Hex() == hostPlayer.Hex() ||
                                         SessionManager.Instance.RemotePlayer.Address.Hex() == hostPlayer.Hex();
            if (isCurrentBoard || isHostPlayerInSession)
            {
                CustomLogger.LogEvent($"[GameFinished]");
                Coroutines.StartRoutine(GameFinishedDelayed());
                CloseAllStructure();
            }
        }

        private IEnumerator GameFinishedDelayed()
        {
            yield return new WaitForSeconds(6f);
            SimpleStorage.ClearCurrentBoardId();
            GameUI.Instance.ShowResultPopUp();
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

            ContestAnimation(root, new ushort[] {blue_points, red_points}, UpdateBoardAfterRoadContest);
            


            CustomLogger.LogEvent(
                $"[RoadContestWon] | Player: {winner} | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private void RoadContestDraw(evolute_duel_RoadContestDraw eventModel)
        {
            string board_id = eventModel.board_id.Hex();

            if (LocalPlayerBoard.id.Hex() != board_id) return;

            byte root = eventModel.root;
            ushort red_points = eventModel.red_points;
            ushort blue_points = eventModel.blue_points;
            
            
            ContestAnimation(root, new ushort[] {blue_points, red_points}, UpdateBoardAfterRoadContest);
            

            CustomLogger.LogEvent(
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

            ContestAnimation(root, new ushort[] {blue_points, red_points}, UpdateBoardAfterCityContest);


            CustomLogger.LogEvent(
                $"[CityContestWon] | Player: {winner} | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }

        private void CityContestDraw(evolute_duel_CityContestDraw eventModel)
        {
            string board_id = eventModel.board_id.Hex();

            if (LocalPlayerBoard.id.Hex() != board_id) return;

            byte root = eventModel.root;
            ushort red_points = eventModel.red_points;
            ushort blue_points = eventModel.blue_points;

            ContestAnimation(root, new ushort[] {blue_points, red_points}, UpdateBoardAfterCityContest);

            CustomLogger.LogEvent(
                $"[CityContestDraw] | BluePoints: {blue_points} | RedPoints: {red_points} | BoardId: {board_id}");
        }
        
        private void GameCanceled(evolute_duel_GameCanceled eventModel)
        {
            string hostPlayer = eventModel.host_player.Hex();
            if(hostPlayer != SessionManager.Instance.LocalPlayer.Address.Hex() &&
               hostPlayer != SessionManager.Instance.RemotePlayer.Address.Hex()) return;
            
            SimpleStorage.ClearCurrentBoardId();
            CustomSceneManager.Instance.LoadLobby();
            // AlertPopupUI.IsGameCanceled = true;
            //GameUI.Instance.ShowResultPopUp();
        }

        
        private ClashAnimation CreateContestAnimation()
        {
            Vector3 offset = new Vector3(0, 0.5f, 0);
            GameObject contestAnimationGO = PrefabsManager.Instance.InstantiateObject(PrefabsManager.Instance.ClashAnimationPrefab);
            ClashAnimation contestAnimation = contestAnimationGO.GetComponent<ClashAnimation>();
            return contestAnimation;
        }
        private void ContestAnimation(byte root, ushort[] points, Action recoloring)
        {
            ClashAnimation contestAnimation = CreateContestAnimation();
            Vector2Int coord = OnChainBoardDataConverter.GetPositionByRoot(root);
            CustomLogger.LogWarning($"ContestAnimation: {coord}, Root: {root}");
            GameObject tile = SessionManager.Instance.Board.GetTileObject(coord.x, coord.y);
            if (tile)
            {
                Vector3 position = tile.transform.position;
                Vector3 offset = new Vector3(0, 0.5f, 0);
                int winner;
                if (points[0] > points[1])
                    winner = 0;
                else if (points[0] < points[1]) 
                    winner = 1;
                else
                    winner = -1;
                contestAnimation.Initialize(position + offset, winner, points, recoloring);
            }
            else
            {
                Coroutines.StartRoutine(RemoteContestAnimation(coord, points, contestAnimation, recoloring));
            }
            
        }

        private Dictionary<evolute_duel_CityNode, List<evolute_duel_CityNode>> cities;
        private Dictionary<evolute_duel_RoadNode, List<evolute_duel_RoadNode>> roads;

        private IEnumerator RemoteContestAnimation(Vector2Int coord, ushort[] points, ClashAnimation contestAnimation, Action recoloring)
        {
            int i = 0;
            int maxAttempts = 6;
            while (i < maxAttempts)
            {
                GameObject tile = SessionManager.Instance.Board.GetTileObject(coord.x, coord.y);
                if(tile){
                    Vector3 position = tile.transform.position;
                    Vector3 offset = new Vector3(0, 0.5f, 0);
                    int winner;
                    if (points[0] > points[1])
                        winner = 0;
                    else if (points[0] < points[1]) 
                        winner = 1;
                    else
                        winner = -1;
                    contestAnimation.Initialize(position + offset, winner, points, recoloring);
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
            CustomLogger.LogWarning("CityNodesList: " + cityNodesList.Count);
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

        private void BuildRoadSets()
        {
            roads = new Dictionary<evolute_duel_RoadNode, List<evolute_duel_RoadNode>>();
            var roadNodesList = GetRoadNodes();
            CustomLogger.LogWarning("RoadNodesList: " + roadNodesList.Count);
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

        public void UpdateBoardAfterCityContest()
        {
            BuildCitySets();
            
            string s = "";
            
            foreach (var city in cities)
            {
                s += "Root: " + city.Key.position + " | ";
                foreach (var node in city.Value)
                {
                    s += node.position + " ";
                    Vector2Int position = OnChainBoardDataConverter.GetPositionByRoot(node.position);
                    TileGenerator tileGenerator = SessionManager.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>();
                    int playerOwner;
                    if(city.Key.contested) playerOwner = city.Key.blue_points > city.Key.red_points ? 0 : 1;
                    else
                    {
                        playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
                    }
                    tileGenerator.RecolorHouses(playerOwner);
                    SessionManager.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y);
                }
                
                
            }
            CustomLogger.LogWarning(s);
        }
        
            
        public void UpdateBoardAfterRoadContest()
        {
            BuildRoadSets();
            
            string s = "";
            foreach (var road in roads)
            {
                s += "Root: " + road.Key.position + " | ";
                foreach (var node in road.Value)
                {
                    s += node.position + " ";
                    (Vector2Int position, Side side) = OnChainBoardDataConverter.GetPositionAndSide(node.position);
                    CustomLogger.LogInfo($"Board: " + SessionManager.Instance.Board);
                    CustomLogger.LogInfo($"TileObject: " + SessionManager.Instance.Board.GetTileObject(position.x, position.y));
                    CustomLogger.LogInfo($"TileGenerator: " + SessionManager.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>());
                    TileGenerator tileGenerator = SessionManager.Instance.Board.GetTileObject(position.x, position.y).GetComponent<TileGenerator>();
                    int playerOwner;
                    if (road.Key.contested)
                    {
                        if (road.Key.blue_points == road.Key.red_points)
                        {
                            continue;
                        }
                        playerOwner = road.Key.blue_points > road.Key.red_points ? 0 : 1;
                    }
                    else
                    {
                        playerOwner = OnChainBoardDataConverter.WhoPlaceTile(LocalPlayerBoard, position);
                    }
                    tileGenerator.RecolorPinOnSide(playerOwner, (int)side);
                    SessionManager.Instance.Board.CheckAndConnectEdgeStructure(playerOwner, position.x, position.y);
                }
                
            }
            CustomLogger.LogWarning(s);
            
        }

        public void CloseAllStructure()
        {
            SessionManager.Instance.Board.CloseAllStructures();
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
                    if(cityNode.board_id.Hex() == LocalPlayerBoard.id.Hex())
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
                    if(roadNode.board_id.Hex() == LocalPlayerBoard.id.Hex())
                        roadNodes.Add(roadNode);
                }
            }
            return roadNodes;
        }

        public evolute_duel_Board GetLocalPlayerBoard()
        {
            return GetBoard(_dojoGameManager.LocalBurnerAccount.Address.Hex());
        }

        public evolute_duel_Board GetBoard(string playerAddress)
        {
            GameObject[] boardsGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Board>();
            foreach (var boardGO in boardsGO)
            {
                if (boardGO.TryGetComponent(out evolute_duel_Board board))
                {
                    //public (FieldElement, PlayerSide, byte, bool) player1;
                    if (board.player1.Item1.Hex() == playerAddress|| board.player2.Item1.Hex() == playerAddress)
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
            CustomLogger.LogInfo("Get top tile. Board id: " + LocalPlayerBoard.id.Hex());
            return new TileData(OnChainBoardDataConverter.GetTopTile(LocalPlayerBoard.top_tile));
        }

        public async void MakeMove(TileData data, int x, int y, bool isJoker)
        {
            Account account = _dojoGameManager.LocalBurnerAccount;
            var tileConfig = OnChainBoardDataConverter.GetTypeAndRotation(data.id);
            Option<byte> jokerTile = isJoker ? new Option<byte>.Some(tileConfig.Item1) : new Option<byte>.None();
            byte rotation = (byte)((tileConfig.Item2 + 1) % 4);
            byte col = (byte) (x - 1);
            byte row = (byte) (y - 1);
            try
            {
                var txHash = await _dojoGameManager.GameSystem.make_move(account, jokerTile, rotation, col, row);
                CustomLogger.LogEvent($"[Make Move]: Hash {txHash} Account {account.Address.Hex()} made a move at {x}, {y}. Rotation: {rotation}");
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error making move: {e.Message}. " +
                                      $"\n| account: {account.Address.Hex()} |" +
                                      $"jokerTile: {jokerTile} |" +
                                      $"rotation: {rotation} |" +
                                      $"col: {col} |" +
                                      $"row: {row} |" +
                                      $"tile config: {data}");
            }
        }
        
        public void CreateSnapshot()
        {
            try
            {
                GameObject[] movesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Move>();
                var txHash = _dojoGameManager.GameSystem.create_snapshot(_localPlayerAccount, LocalPlayerBoard.id, (byte)movesGO.Length);
                CustomLogger.LogEvent($"[Create Snapshot]: Hash {txHash} Account {_localPlayerAccount.Address.Hex()} created a snapshot");
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Error creating snapshot: {e.Message}. " +
                                      $"\n| account: {_localPlayerAccount.Address.Hex()} |" +
                                      $"boardId: {LocalPlayerBoard.id} |" +
                                      $"snapshotTurn: {_snapshotTurn}");
            }
        }

        public async void SkipMove()
        {
            try
            {
                var txHash = await _dojoGameManager.GameSystem.skip_move(_localPlayerAccount);
                CustomLogger.LogEvent($"[Skip Move]: Hash {txHash} Account {_localPlayerAccount.Address.Hex()} skipped a move");
                
            } catch (Exception e)
            {
                CustomLogger.LogError($"Error skipping move: {e.Message}");
            }
        }

        private evolute_duel_Move GetMoveModelById(Option<FieldElement> move_id)
        {
            GameObject[] movesGO = _dojoGameManager.WorldManager.Entities<evolute_duel_Move>();
            foreach (var moveGO in movesGO)
            {
                if (moveGO.TryGetComponent(out evolute_duel_Move move))
                {
                    string moveId = move_id switch
                    {
                        Option<FieldElement>.Some id => id.value.Hex(),
                        Option<FieldElement>.None => null
                    };
                    if (moveId == null) continue;
                    if (move.id.Hex() == moveId)
                    {
                        return move;
                    }
                }
            }
            return null;
            
        }
        
        
    }
}