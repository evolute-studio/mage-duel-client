using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
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

        public bool IsLocalPlayerHost = true;
        
        public SessionContext SessionContext = new SessionContext();
        public SessionManagerContext ManagerContext { get; private set; }
        private List<ISessionComponent> _components;
        
        [Header("Dependencies")]
        public BoardManager BoardManager;
        public TileSelector TileSelector;
        

        private async void Start()
        {
            await SetupData();
            InitializeBoard();
            Initialize();
            GameUI.Instance.Initialize();
            CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            ManagerContext.GameLoopManager.StartGame();
        }

        private void Initialize()
        {
            ManagerContext = new SessionManagerContext();
            _components = new List<ISessionComponent>();

            var playersManager = new PlayersManager();
            var gameLoopManager = new GameLoopManager();
            var jokerManager = new JokerManager();
            

            ManagerContext.SessionContext = SessionContext;
            ManagerContext.SessionManager = this;
            ManagerContext.PlayersManager = playersManager;
            ManagerContext.GameLoopManager = gameLoopManager;
            ManagerContext.JokerManager = jokerManager;

            _components.Add(playersManager);
            _components.Add(gameLoopManager);
            _components.Add(jokerManager);

            foreach (var component in _components)
                component.Initialize(ManagerContext);
        }
        
        public async Task SetupData()
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
            //IncomingModelsFilter.AllowedBoards.Add("0x0000000000000000000000000000000000000000000000000000000000000038");
            //Board board = await DojoLayer.Instance.GetBoard("0x0000000000000000000000000000000000000000000000000000000000000038");
            if (board.IsNull)
            {
                CustomLogger.LogError("[SessionManager.SetupData] - Board is null");
                CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                return;
            }
            SessionContext.Board = board;
            SimpleStorage.SaveCurrentBoardId(board.Id);
            SessionContext.PlayersData[0] = board.Player1;
            SessionContext.PlayersData[1] = board.Player2;
            PlayerProfile player1 = await DojoLayer.Instance.GetPlayerProfile(board.Player1.PlayerId);
            PlayerProfile player2 = await DojoLayer.Instance.GetPlayerProfile(board.Player2.PlayerId);
            SessionContext.PlayersData[0].SetData(player1);
            SessionContext.PlayersData[1].SetData(player2);
            
            DojoGameManager.Instance.GlobalContext.SessionContext = SessionContext;
        }
        
        private void InitializeBoard()
        {
            var board = SessionContext.Board;
            BoardManager.Initialize(board);
        }
        
        private void OnDestroy()
        {
            DojoGameManager.Instance.GlobalContext.SessionContext = null;
            foreach (var component in _components)
                component.Dispose();
        }
    }
}