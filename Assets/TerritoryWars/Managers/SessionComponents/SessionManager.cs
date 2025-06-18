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
        public bool IsInitialized = false;

        [Header("Dependencies")]
        public BoardManager BoardManager;
        public TileSelector TileSelector;
        public StructureHoverManager StructureHoverManager;
        
        //for debug
        [SerializeField] public bool IsBotSkipping = false;


        private async void Start()
        {
            ApplicationState.CurrentState = ApplicationStates.Initializing;
            Initialize();
            await SetupData();
            ManagerContext.PlayersManager.Initialize(ManagerContext);
            ManagerContext.ContestManager.Initialize(ManagerContext);
            InitializeBoard();
            ManagerContext.GameLoopManager.Initialize(ManagerContext);
            ManagerContext.JokerManager.Initialize(ManagerContext);
            

            GameUI.Instance.Initialize();
            GameUI.Instance.playerInfoUI.Initialize();
            GameUI.Instance.playerInfoUI.UpdateData(SessionContext.PlayersData);
            GameUI.Instance.playerInfoUI.SetDeckCount(SessionContext.Board.AvailableTilesInDeck.Length);

            CustomSceneManager.Instance.LoadingScreen.SetActive(false);



            ManagerContext.GameLoopManager.StartGame();
            ApplicationState.CurrentState = ApplicationStates.Session;
            IsInitialized = true;
        }

        private void Initialize()
        {
            ManagerContext = new SessionManagerContext();
            _components = new List<ISessionComponent>();

            var playersManager = new PlayersManager();
            var gameLoopManager = new GameLoopManager();
            var jokerManager = new JokerManager();
            var contestManager = new ContestManager();


            ManagerContext.SessionContext = SessionContext;
            ManagerContext.SessionManager = this;
            ManagerContext.PlayersManager = playersManager;
            ManagerContext.GameLoopManager = gameLoopManager;
            ManagerContext.JokerManager = jokerManager;
            ManagerContext.ContestManager = contestManager;

            _components.Add(playersManager);
            _components.Add(gameLoopManager);
            _components.Add(jokerManager);
            _components.Add(contestManager);
        }

        public async Task SetupData()
        {
            CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Starting SetupData");
            SessionContext.LocalPlayerAddress = DojoGameManager.Instance.LocalAccount.Address.Hex();
            //GameModel fromGlobalContext = DojoGameManager.Instance.GlobalContext.GameInProgress;
            Board boardForLoad = DojoGameManager.Instance.GlobalContext.BoardForLoad;
            if (boardForLoad.IsNull)
            {
                GameModel game = await DojoLayer.Instance.GetGameInProgress(SessionContext.LocalPlayerAddress);
                if (game.IsNull || game.BoardId == null)
                {
                    CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Game is null or BoardId is null. Retrying...");
                    await Coroutines.CoroutineAsync(() => { }, 1f);
                    game = await DojoLayer.Instance.GetGameInProgress(SessionContext.LocalPlayerAddress);
                    if (game.BoardId == null)
                    {
                        CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Game is still null or BoardId is null after retry. Redirecting to menu.");
                        CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                    }
                }
                SessionContext.Game = game;
            }
            else
            {
                DojoGameManager.Instance.GlobalContext.BoardForLoad = default;
            }
            
            CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Game retrieved successfully");
            Board board = boardForLoad.IsNull ? await DojoLayer.Instance.GetBoard(SessionContext.Game.BoardId) : boardForLoad;
            //IncomingModelsFilter.AllowedBoards.Add("0x0000000000000000000000000000000000000000000000000000000000000038");
            //Board board = await DojoLayer.Instance.GetBoard("0x0000000000000000000000000000000000000000000000000000000000000038");
            if (board.IsNull)
            {
                CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Board is null. Redirecting to menu.");
                CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                return;
            }
            CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Board retrieved successfully");
            UnionFind unionFind = await DojoLayer.Instance.GetUnionFind(board.Id);
            CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Union Find retrieved successfully");

            SessionContext.Board = board;
            SessionContext.UnionFind = unionFind;
            SimpleStorage.SaveCurrentBoardId(board.Id);
            SessionContext.PlayersData[0] = board.Player1;
            SessionContext.PlayersData[1] = board.Player2;
            PlayerProfile player1 = await DojoLayer.Instance.GetPlayerProfile(board.Player1.PlayerId);
            CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Player 1 retrieved successfully");
            PlayerProfile player2 = await DojoLayer.Instance.GetPlayerProfile(board.Player2.PlayerId);
            CustomLogger.LogDojoLoop("[SessionManager.SetupData] - Player 2 retrieved successfully");
            SessionContext.PlayersData[0].SetData(player1);
            SessionContext.PlayersData[1].SetData(player2);

            SessionContext.IsGameWithBot = DojoGameManager.Instance.DojoSessionManager.IsGameWithBot;
            SessionContext.IsGameWithBotAsPlayer = DojoGameManager.Instance.DojoSessionManager.IsGameWithBotAsPlayer;
            
            IsLocalPlayerHost = SessionContext.LocalPlayerAddress == board.Player1.PlayerId;

            DojoGameManager.Instance.GlobalContext.SessionContext = SessionContext;
            CustomLogger.LogDojoLoop("[SessionManager.SetupData] - SessionContext initialized successfully");
        }

        private void InitializeBoard()
        {
            var board = SessionContext.Board;
            BoardManager.Initialize(board);
            ManagerContext.ContestManager.RecolorStructures();
        }

        private void OnDestroy()
        {
            DojoGameManager.Instance.GlobalContext.SessionContext = null;
            foreach (var component in _components)
                component.Dispose();
        }
    }
}
