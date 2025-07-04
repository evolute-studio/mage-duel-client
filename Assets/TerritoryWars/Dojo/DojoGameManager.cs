using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dojo;
using Dojo.Starknet;
using UnityEngine;

// fix to use Records in Unity ref. https://stackoverflow.com/a/73100830
using System.ComponentModel;
using System.Threading.Tasks;
using TerritoryWars.Bots;
using TerritoryWars.ConnectorLayers;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Managers;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.SaveStorage;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal class IsExternalInit { }
}

namespace TerritoryWars.Dojo
{
    public class DojoGameManager : MonoBehaviour
    {
        public static DojoGameManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        
        public WorldManager WorldManager;
        public CustomSynchronizationMaster CustomSynchronizationMaster;

        public WorldManagerData dojoConfig;
        [SerializeField] GameManagerData gameManagerData;
        
        public Game GameSystem;
        public Player_profile_actions PlayerProfileSystem;

        public BurnerManager burnerManager;

        public JsonRpcClient provider;
        public Account masterAccount;
        
        public GeneralAccount LocalAccount { get; private set; }
        public Bot LocalBot { get; private set; }
        public Bot LocalBotAsPlayer { get; private set; }

        public bool IsLocalPlayer;
        
        public DojoSessionManager DojoSessionManager;
        
        public UnityEvent OnLocalPlayerSet = new UnityEvent();

        private evolute_duel_Game _gameInProgress;
        private evolute_duel_Board _boardInProgress;
        
        public GlobalContext GlobalContext { get; private set; } = new GlobalContext();
        
        public evolute_duel_Game GameInProgress
        {
            get
            {
                if(_gameInProgress != null) return _gameInProgress;
                evolute_duel_Game game = WorldManager.Entities<evolute_duel_Game>()
                    .FirstOrDefault(g =>
                        g.GetComponent<evolute_duel_Game>().player.Hex() == LocalAccount.Address.Hex())?
                    .GetComponent<evolute_duel_Game>();
                if (game == null)
                {
                    return null;
                }
                _gameInProgress = game;
                return game;
            }
            set => _gameInProgress = value;
        }
        
        public evolute_duel_Board BoardInProgress
        {
            get
            {
                if(_boardInProgress != null) return _boardInProgress;
                evolute_duel_Board board = WorldManager.Entities<evolute_duel_Board>()
                    .FirstOrDefault(b =>
                        b.GetComponent<evolute_duel_Board>()?.player1.Item1?.Hex() == LocalAccount.Address.Hex() ||
                        b.GetComponent<evolute_duel_Board>()?.player2.Item1?.Hex() == LocalAccount.Address.Hex())?
                    .GetComponent<evolute_duel_Board>();
                if (board == null)
                {
                    return null;
                }
                _boardInProgress = board;
                return board;
            }
            set => _boardInProgress = value;
        }

        public void Start()
        {
            EventBus.Subscribe<ErrorOccured>(OnGameJoinFailed);
        }


        public void SetupMasterAccount(Action callback, WrapperConnectorCalls.ConnectionData connection = default)
        {
            try 
            {
                string rpcUrl =  String.IsNullOrEmpty(connection.rpcUrl) ? dojoConfig.rpcUrl: connection.rpcUrl;
                provider = new JsonRpcClient(rpcUrl);
                masterAccount = new Account(provider, new SigningKey(gameManagerData.masterPrivateKey),
                    new FieldElement(gameManagerData.masterAddress), callback);
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Failed to setup account: {e}");
                throw;
            }
        }

        public async Task CreateBurners()
        {
            burnerManager = new BurnerManager(provider, masterAccount);
            await burnerManager.LoadBurnersFromStorage();
            
            WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
            WorldManager.synchronizationMaster.OnSynchronized.AddListener(OnSynchronized);
            WorldManager.synchronizationMaster.OnEntitySpawned.AddListener(SpawnEntity);
            WorldManager.synchronizationMaster.OnModelUpdated.AddListener(IncomingModelsFilter.FilterModels);
        }

        public void SetLocalControllerAccount(FieldElement address)
        {
            LocalAccount = new GeneralAccount(address);
            SimpleStorage.SetPlayerAddress(LocalAccount.Address.Hex());
            IncomingModelsFilter.SetLocalPlayerId(LocalAccount.Address.Hex());
        }
        
        public async Task CreateLocalPlayer()
        {
            Account account = await TryCreateAccount(3, false);
            LocalAccount = new GeneralAccount(account);
            SimpleStorage.SetPlayerAddress(LocalAccount.Address.Hex());
            IncomingModelsFilter.SetLocalPlayerId(LocalAccount.Address.Hex());
        }

        public async Task CreateBot()
        {
            LocalBot = await GetBotForGame(false);
            if (LocalBot == null)
            {
                CustomLogger.LogError("Failed to create bot");
            }
        }
        
        public Bot CreateBotAsPlayer()
        {
            var localBotAsPlayer = new Bot();
            localBotAsPlayer.Initialize(LocalAccount);
            return localBotAsPlayer;
            //DojoConnector.BecameBot(LocalBurnerAccount);
        }

        public async Task SyncInitialModels()
        {
            int count = 0;
            (Rules rules, Shop shop) = await DojoModels.GetGeneralModels();
            GlobalContext.Rules = rules;
            GlobalContext.Shop = shop;
            
            PlayerProfile playerProfile = await DojoModels.GetPlayerProfile(LocalAccount.Address.Hex());
            GlobalContext.PlayerProfile = playerProfile;
            
            GameModel game = await DojoModels.GetGameInProgress(LocalAccount.Address.Hex());
            GlobalContext.GameInProgress = game;
        }

        public async Task SyncEverythingForGame()
        {
            CustomLogger.LogDojoLoop("SyncEverythingForGame");
            await CustomSynchronizationMaster.SyncPlayerInProgressGame(LocalAccount.Address);
            evolute_duel_Game game = GameInProgress;
            FieldElement boardId = game.board_id switch
            {
                Option<FieldElement>.Some some => some.value,
                _ => null
            };
            IncomingModelsFilter.SetSessionCurrentBoardId(boardId.Hex());
            CustomLogger.LogDojoLoop("SyncEverythingForGame. BoardId: " + boardId.Hex());
            int count = await CustomSynchronizationMaster.SyncBoardWithDependencies(boardId);
            CustomLogger.LogInfo("Board synced: " + WorldManager.Entities<evolute_duel_Board>().Length);
            evolute_duel_Board board = BoardInProgress;
            FieldElement[] players = new FieldElement[] { board.player1.Item1, board.player2.Item1 };
            IncomingModelsFilter.SetSessionPlayers(players.Select(p => p.Hex()).ToList());
            await CustomSynchronizationMaster.SyncPlayersArray(players);
            var allowedBoards = await CustomSynchronizationMaster.SyncAllMoveByBoardId(board.id);
            if (allowedBoards != null)
                IncomingModelsFilter.SetSessionAllowedBoards(allowedBoards.ToList());
            
            
            CustomLogger.LogDojoLoop("SyncEverythingForGame. Synced boards: " + count);
        }
        
        // private async Task<bool> CheckGameInProgress()
        // {
        //     int boardCount = 0;
        //     GameObject[] games = WorldManager.Entities<evolute_duel_Game>();
        //     if (games.Length > 0)
        //     {
        //         evolute_duel_Game game = GameInProgress;
        //         if (game == null)
        //         {
        //             CustomLogger.LogError("Failed to load game model");
        //             return false;
        //         }
        //         var player = await CustomSynchronizationMaster.WaitForModelByPredicate<evolute_duel_Player>(
        //             p => p.player_id.Hex() == game.player.Hex()
        //         );
        //         
        //         if (player == null)
        //         {
        //             CustomLogger.LogError("Failed to load player model for game");
        //             return false;
        //         }
        //
        //         FieldElement boardId = game.board_id switch
        //         {
        //             Option<FieldElement>.Some some => some.value,
        //             _ => null
        //         };
        //         
        //         boardCount = await CustomSynchronizationMaster.SyncBoardWithDependencies(boardId);
        //     }
        //     return boardCount > 0;
        // }

        public void LoadGame()
        {
            if (GlobalContext.HasGameInProgress)
                LoadSession();
            else
                LoadMenu();
            
        }

        public void LoadSession()
        {
            DojoSessionManager?.OnDestroy();
            DojoSessionManager = new DojoSessionManager(this);
            CustomSceneManager.Instance.LoadSession(
                startAction: () =>
                    CustomSceneManager.Instance.LoadingScreen.SetActive(true, 
                        () => DojoConnector.CancelGame(DojoGameManager.Instance.LocalAccount), 
                        LoadingScreen.connectingText),
                finishAction: () =>
                    CustomSceneManager.Instance.LoadingScreen.SetActive(false));
        }

        private void LoadMenu()
        {
            CustomSceneManager.Instance.LoadLobby(
                startAction: () =>
                    CustomSceneManager.Instance.LoadingScreen.SetActive(true, null, LoadingScreen.launchGameText),
                finishAction: () =>
                    CustomSceneManager.Instance.LoadingScreen.SetActive(false));
        }
        
        
        public async Task SyncCreatedGames()
        {
            int count = await CustomSynchronizationMaster.SyncCreatedGame();
            if (count == 0)
            {
                CustomLogger.LogInfo("No created games found"); 
                return;
            }
            await SyncPlayerModelsForGames();
        }

        public async Task SyncInProgressGames()
        {
            int count = await CustomSynchronizationMaster.SyncAllGameInProgress();
            if (count == 0)
            {
                CustomLogger.LogInfo("No created games found"); 
                return;
            }
        }

        public async Task SyncPlayerModelsForGames()
        {
            GameObject[] gameGOs = WorldManager.Entities<evolute_duel_Game>();
            List<FieldElement> hostPlayersAddresses = new List<FieldElement>();
            List<FieldElement> snapshotIds = new List<FieldElement>();
            foreach (var gameGO in gameGOs)
            {
                CustomLogger.LogInfo("Game found");
                if (gameGO.TryGetComponent(out evolute_duel_Game game))
                {
                    CustomLogger.LogInfo("Adding host player to the list. Address: " + game.player.Hex());
                    hostPlayersAddresses.Add(game.player);
                    var snapshotId = game.snapshot_id switch
                    {
                        Option<FieldElement>.Some some => some.value,
                        _ => null
                    };
                    if (snapshotId != null)
                    {
                        snapshotIds.Add(snapshotId);
                    }
                }
            }
            IncomingModelsFilter.AddRangePlayersToAllowedPlayers(hostPlayersAddresses.Select(p => p.Hex()).ToList());
            int count = await CustomSynchronizationMaster.SyncPlayersArray(hostPlayersAddresses.ToArray());
            Debug.Log($"Synced {count} players");
            count = await CustomSynchronizationMaster.SyncSnapshotArray(snapshotIds.ToArray());
            Debug.Log($"Synced {count} snapshots");
            
        }
        
        public async Task SyncLocalPlayerSnapshots()
        {
            int count = await CustomSynchronizationMaster.SyncPlayerSnapshots(LocalAccount.Address);
        }
        
        #region Account Creation 
        private async Task<Account> TryCreateAccount(int attempts, bool createNew)
        {
            try
            {
                for (int i = 0; i < attempts; i++)
                {
                    CustomLogger.LogInfo($"Creating burner account. Attempt: {i}");
                    Account account = await CreateAccount(createNew);
                    if (account == null)
                    {
                        CustomLogger.LogInfo($"Burner account created. Attempt: {i}. Address: {LocalAccount.Address.Hex()}");
                        OnLocalPlayerSet?.Invoke();
                        break;
                    }

                    return account;
                }
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Failed to create burner account. {e}");
            }
            CustomLogger.LogError("Failed to create burner account");
            return null;
        }
        
        
        public async Task<Account> CreateAccount(bool createNew)
        {
            try
            {
                if (createNew)
                {
                    return await burnerManager.DeployBurner();
                }
                else
                {
                    string storedAccountAddress = SimpleStorage.LoadPlayerAddress();
                    bool isBurnersEmpty = burnerManager.Burners.Count == 0;
                    Account storedAccount = burnerManager.Burners.FirstOrDefault(b => b.Address.Hex() == storedAccountAddress);
                    if (storedAccount == null || isBurnersEmpty)
                    {
                        CustomLogger.LogWarning("Burner account not found. Creating new account.");
                        return await burnerManager.DeployBurner();
                    }
                    else
                    {
                        return storedAccount;
                    }
                }
                SimpleStorage.SetPlayerAddress(LocalAccount.Address.Hex());
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public async Task<Account> CreateAccount()
        {
            try
            {
                Account account = await burnerManager.DeployBurner();
                return account;
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Failed to create burner account. {e}");
                return null;
            }
        }

        public bool TryGetAccount(string address, out Account account)
        {
            account = burnerManager.Burners.FirstOrDefault(b => b.Address.Hex() == address);
            if (account == null)
            {
                return false;
            }
            return true;
            
        }
        #endregion

        public async void CreateGameWithBots()
        {
            EventBus.Subscribe<GameUpdated>(BotJoinToPlayer);
            
            CustomLogger.LogDojoLoop("CreateGameWithBots");
            LocalBot ??= await GetBotForGame(false);
            CustomLogger.LogDojoLoop("Bot created");
            if (LocalBot == null)
            {
                CustomLogger.LogError("Failed to create bot");
                return;
            }

            await DojoConnector.ChangeUsername(LocalBot.Account,
               new FieldElement(LocalBot.AccountModule.GetDefaultUsername(), true));
            CustomLogger.LogDojoLoop("Bot username changed");
            await DojoConnector.CreateGame(LocalAccount);
            CustomLogger.LogDojoLoop("Game created");

        }

        private async void BotJoinToPlayer(GameUpdated gameUpdated)
        {
            if(gameUpdated.PlayerId != LocalAccount.Address.Hex() || gameUpdated.Status != GameModelStatus.Created) return;
            await DojoConnector.JoinGame(LocalBot.Account, LocalAccount.Address);
            CustomLogger.LogDojoLoop("Bot joined game");
            EventBus.Unsubscribe<GameUpdated>(BotJoinToPlayer);
        }
        
        [ContextMenu("Create game between bots")]
        public async void CreateGameBetweenBots()
        {
            CustomLogger.LogDojoLoop("CreateGameBetweenBots");
            LocalBot ??= await GetBotForGame(false);
            if (LocalBot == null)
            {
                CustomLogger.LogError("Failed to create bot");
                return;
            }

            LocalBotAsPlayer ??= CreateBotAsPlayer();
            await DojoConnector.ChangeUsername(LocalBot.Account,
                new FieldElement(LocalBot.AccountModule.GetDefaultUsername(), true));
            CustomLogger.LogDojoLoop("Bot username changed");
            await DojoConnector.CreateGame(LocalBotAsPlayer.Account);
            CustomLogger.LogDojoLoop("Game created");
            DojoConnector.JoinGame(LocalBot.Account, LocalBotAsPlayer.Account.Address);
        }
        public async Task<Bot> GetBotForGame(bool newBot)
        {
            Account account;
            if (newBot)
            {
                account = await CreateAccount();
                SimpleStorage.SetBotAddress(account.Address.Hex());
            }
            else
            {
                if (!TryGetAccount(SimpleStorage.LoadBotAddress(), out account))
                {
                    account = await CreateAccount();
                    SimpleStorage.SetBotAddress(account.Address.Hex());
                };
            }
            
            if (account == null)
            {
                CustomLogger.LogError("Failed to create bot account");
                return null;
            }
            Bot bot = new Bot();
            GeneralAccount botAccount = new GeneralAccount(account);
            bot.Initialize(botAccount);
            DojoConnector.BecameBot(botAccount);
            return bot;
        }

        public void OnGameJoinFailed(ErrorOccured error)
        {
            EventBus.Subscribe<GameUpdated>(TryToRecreateMatch);
            DojoConnector.CancelGame(LocalAccount);
        }

        private async void TryToRecreateMatch(GameUpdated updated)
        {
            if(updated.Status != GameModelStatus.Canceled) return;
            if (SimpleStorage.LoadIsGameWithBot())
            {
                CreateGameWithBots();
            }
            else
            {
                await DojoConnector.CreateGame(LocalAccount);
            }
            
            EventBus.Unsubscribe<GameUpdated>(TryToRecreateMatch);
        }
        

        public GameObject[] GetGames() => WorldManager.Entities<evolute_duel_Game>();
        public GameObject[] GetSnapshots() => WorldManager.Entities<evolute_duel_Snapshot>();
        
        public evolute_duel_Snapshot GetSnapshot(FieldElement snapshotId)
        {
            if (snapshotId == null) return null;
            GameObject[] snapshots = GetSnapshots();
            foreach (var snapshot in snapshots)
            {
                if (snapshot.TryGetComponent(out evolute_duel_Snapshot snapshotModel))
                {
                    if (snapshotModel.snapshot_id.Hex() == snapshotId.Hex())
                    {
                        return snapshotModel;
                    }
                }
            }
            return null;
        }
        
        private void OnEventMessage(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                case evolute_duel_PlayerUsernameChanged playerUsernameChanged:
                    PlayerUsernameChanged(playerUsernameChanged);
                    break;
                case evolute_duel_GameCreateFailed gameCreateFailed:
                    GameCreateFailed(gameCreateFailed);
                    break;
                case evolute_duel_GameStarted gameStarted:
                    CheckStartSession(gameStarted);
                    break;
            }
        }
        
        private void PlayerUsernameChanged(evolute_duel_PlayerUsernameChanged eventMessage)
        {
            if(LocalAccount == null || LocalAccount.Address.Hex() != eventMessage.player_id.Hex()) return;
            if(CairoFieldsConverter.GetStringFromFieldElement(eventMessage.new_username).StartsWith("Guest")) return;
            MenuUIController.Instance.NamePanelController.SetName(CairoFieldsConverter.GetStringFromFieldElement(eventMessage.new_username));
        }
        
        private void GameCreateFailed(evolute_duel_GameCreateFailed eventMessage)
        {
            string hostPlayer = eventMessage.host_player.Hex();
            if (LocalAccount.Address.Hex() != hostPlayer) return;
            DojoConnector.CancelGame(LocalAccount);
            InvokeWithDelay(() => DojoConnector.CreateGame(LocalAccount), 1f);
        }
        

        private async void CheckStartSession(ModelInstance eventMessage)
        {
            evolute_duel_GameStarted gameStarted = eventMessage as evolute_duel_GameStarted;
            if (gameStarted == null)
            {
                CustomLogger.LogWarning($"Event {nameof(evolute_duel_Game)} is null");
                return;
            }
            CustomLogger.LogInfo($"Check start session. " +
                                 $"\nLocalPlayerIsHost: {gameStarted.host_player.Hex() == LocalAccount.Address.Hex()}" +
                                 $"\nLocalPlayerIsGuest: {gameStarted.guest_player.Hex() == LocalAccount.Address.Hex()}" +
                                 $"\nEventHostPlayerAddress: {gameStarted.host_player.Hex()}" +
                                 $"\nEventGuestPlayerAddress: {gameStarted.guest_player.Hex()}" +
                                 $"\nLocalPlayerAddress: {LocalAccount.Address.Hex()}");
            if (gameStarted.host_player.Hex() == LocalAccount.Address.Hex() ||
                gameStarted.guest_player.Hex() == LocalAccount.Address.Hex())
            {
                CustomLogger.LogInfo("Start session");
                // Start session
                ApplicationState.SetState(ApplicationStates.Initializing);
                //await SyncEverythingForGame();
                DojoSessionManager?.OnDestroy();
                DojoSessionManager = new DojoSessionManager(this);
                CustomSceneManager.Instance.LoadSession();
            }
        }
        
        public bool IsTargetModel(ModelInstance modelInstance, string targetModelName)
        {
            string modelInstanceName = modelInstance.ToString();
            if (modelInstanceName.Contains(targetModelName))
            {
                return true;
            }
            return false;
        }

        public evolute_duel_Player GetPlayerData(string address)
        {
            GameObject[] playerModelsGO = WorldManager.Entities<evolute_duel_Player>();
            foreach (var playerModelGO in playerModelsGO)
            {
                if (playerModelGO.TryGetComponent(out evolute_duel_Player playerModel))
                {
                    if (playerModel.player_id.Hex() == address)
                    {
                        return playerModel;
                    }
                }
            }
            return null;
        }


        public evolute_duel_Move GetMove(FieldElement moveId)
        {
            GameObject[] moveModelsGO = WorldManager.Entities<evolute_duel_Move>();
            foreach (var moveModelGO in moveModelsGO)
            {
                if (moveModelGO.TryGetComponent(out evolute_duel_Move moveModel))
                {
                    if (moveModel.id.Hex() == moveId.Hex())
                    {
                        return moveModel;
                    }
                }
            }
            return null;
        }
        
        public List<evolute_duel_Move> GetMoves(List<evolute_duel_Move> moves, GameObject[] allMoveGameObjects = null)
        {
            CustomLogger.LogInfo($"GetMoves: {moves.Count}");
            if (allMoveGameObjects == null)
            {
                allMoveGameObjects = WorldManager.Entities<evolute_duel_Move>();
            }

            evolute_duel_Move currentMove = moves.First();
            FieldElement previousMoveId = currentMove.prev_move_id switch
            {
                Option<FieldElement>.Some some => some.value,
                _ => null
            };
            if (previousMoveId == null)
            {
                return moves;
            }
            
            foreach (var moveGO in allMoveGameObjects)
            {
                if (moveGO.TryGetComponent(out evolute_duel_Move move))
                {
                    if (move.id.Hex() == previousMoveId.Hex())
                    {
                        moves.Insert(0, move);
                        return GetMoves(moves, allMoveGameObjects);
                    }
                }
            }

            return moves;
        }
        
        public evolute_duel_Player GetLocalPlayerData()
        {
            evolute_duel_Player player = GetPlayerData(LocalAccount.Address.Hex());
            if (player == null)
            {
                CustomLogger.LogWarning("Local player not found. Waiting for player model");
            }

            // player = CustomSynchronizationMaster.WaitForModelByPredicate<evolute_duel_Player>(
            //     p => p.player_id.Hex() == LocalBurnerAccount.Address.Hex()).Result;
            //
            // if (player == null)
            // {
            //     CustomLogger.LogError("Local player not found");
            // }
            return player;
            
        }
        
        private void OnSynchronized(List<GameObject> synchronizedModels)
        {
            CustomLogger.LogInfo($"Synchronized {synchronizedModels.Count} models");
        }
        
        public void SpawnEntity(GameObject entity)
        {
            if (entity == null) return;
            CustomLogger.LogInfo($"Spawned entity: {entity.name}");
        }
        
        
        public evolute_duel_Player GetPlayerProfileByAddress(string address)
        {
            GameObject[] playerProfiles = WorldManager.Entities<evolute_duel_Player>();
            foreach (var playerProfile in playerProfiles)
            {
                if (playerProfile.TryGetComponent(out evolute_duel_Player player))
                {
                    if (player.player_id.Hex() == address)
                    {
                        return player;
                    }
                }
            }
            return null;
        }
        
        
        public void InvokeWithDelay(Action action, float delay)
        {
            StartCoroutine(TryAgainCoroutine(action, delay));
        }
        
        private IEnumerator TryAgainCoroutine(Action action, float delay){
            yield return new WaitForSeconds(delay);
            action();
        }
        
        void OnDestroy()
        {
            if (WorldManager.synchronizationMaster != null)
            {
                WorldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnEventMessage);
                WorldManager.synchronizationMaster.OnSynchronized.RemoveListener(OnSynchronized);
                WorldManager.synchronizationMaster.OnEntitySpawned.RemoveListener(SpawnEntity);
                //WorldManager.synchronizationMaster.OnModelUpdated.RemoveListener(ModelUpdated);
            }
            EventBus.Unsubscribe<ErrorOccured>(OnGameJoinFailed);
        }
    }
}