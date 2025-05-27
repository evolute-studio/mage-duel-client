using System;
using System.Collections.Generic;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }
        
        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Debug.LogError("SessionManager already exists. Deleting new instance.");
                Destroy(gameObject);
            }

            if (!CustomSceneManager.Instance.LoadingScreen.IsLoading)
            {
                CustomSceneManager.Instance.LoadingScreen.SetActive(true, 
                    () => DojoConnector.CancelGame(DojoGameManager.Instance.LocalAccount), 
                    LoadingScreen.connectingText);
            }
        }
        
        public SessionContext SessionContext = new SessionContext();
        private SessionManagerContext _managerContext;
        private List<ISessionComponent> _components;
        
        [Header("Dependencies")]
        public BoardManager BoardManager;
        

        private void Start()
        {
            SetupData();
            Initialize();
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
        }

        private void Initialize()
        {
            _managerContext = new SessionManagerContext();
            _components = new List<ISessionComponent>();

            var playersManager = new PlayersManager();
            var gameLoopManager = new GameLoopManager();

            _managerContext.SessionContext = SessionContext;
            _managerContext.SessionManager = this;
            _managerContext.PlayersManager = playersManager;
            _managerContext.GameLoopManager = gameLoopManager;

            _components.Add(playersManager);
            _components.Add(gameLoopManager);

            foreach (var component in _components)
                component.Initialize(_managerContext);
        }
        
        public async void SetupData()
        {
            SessionContext.LocalPlayerAddress = DojoGameManager.Instance.LocalAccount.Address.Hex();
            GameModel game = await DojoLayer.Instance.GetGameInProgress(SessionContext.LocalPlayerAddress);
            if (game.IsNull)
            {
                CustomLogger.LogError("[SessionManager.SetupData] - Game is null");
                CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                return;
            }
            SessionContext.Game = game;
            Board board = await DojoLayer.Instance.GetBoard(SessionContext.Game.BoardId);
            if (board.IsNull)
            {
                CustomLogger.LogError("[SessionManager.SetupData] - Board is null");
                CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                return;
            }
            SessionContext.Board = board;
            SimpleStorage.SaveCurrentBoardId(board.Id);
            SessionContext.Players[0] = board.Player1;
            SessionContext.Players[1] = board.Player2;
            PlayerProfile player1 = await DojoLayer.Instance.GetPlayerProfile(board.Player1.PlayerId);
            PlayerProfile player2 = await DojoLayer.Instance.GetPlayerProfile(board.Player2.PlayerId);
            SessionContext.Players[0].ActiveSkin = player1.ActiveSkin;
            SessionContext.Players[1].ActiveSkin = player2.ActiveSkin;
        }
        
        private void InitializeBoard()
        {
            var board = SessionContext.Board;
            List<Move> processedMoves = new List<Move>();
            string lastMoveId = board.LastMoveId;
            
            BoardManager.Initialize();
            if (lastMoveId != null)
            {
                evolute_duel_Move lastMove = DojoGameManager.Instance.GetMove(lastMoveId);
                int playerIndex = lastMove.player_side switch
                {
                    PlayerSide.Blue => 1,
                    PlayerSide.Red => 0,
                    _ => -1
                };
                CurrentTurnPlayer = Players[playerIndex];
                List<evolute_duel_Move> moves = DojoGameManager.Instance.GetMoves(new List<evolute_duel_Move>{lastMove});
                int moveNumber = 0;
                foreach (var move in moves)
                {
                    int owner = move.player_side switch
                    {
                        PlayerSide.Blue => 0,
                        PlayerSide.Red => 1,
                        _ => -1
                    };
                    string tileConfig = OnChainBoardDataConverter.GetTopTile(move.tile);
                    if (tileConfig == null) continue;
                    TileData tile = new TileData(tileConfig);
                    int rotation = move.rotation;
                    int x = move.col + 1;
                    int y = move.row + 1;

                    tile.Rotate((rotation + 3) % 4);
                    Board.PlaceTile(tile, x, y, owner);
                    processedMoves.Add(move);
                }

                GameObject[] allMoves = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Move>();
                foreach (var move in allMoves)
                {
                    evolute_duel_Move moveComponent = move.GetComponent<evolute_duel_Move>();
                    if (processedMoves.Contains(moveComponent)) continue;
                    IncomingModelsFilter.DestroyModel(moveComponent);
                }
            } 
        }
        
        private void OnDestroy()
        {
            foreach (var component in _components)
                component.Dispose();
        }
    }
}