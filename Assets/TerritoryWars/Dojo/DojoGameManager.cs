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
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
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
        
        public Account LocalBurnerAccount { get; private set; }
        public Bot LocalBot { get; private set; }

        public bool IsLocalPlayer;
        
        public DojoSessionManager SessionManager;
        
        public UnityEvent OnLocalPlayerSet = new UnityEvent();


        public void SetupMasterAccount(Action callback)
        {
            try 
            {
                provider = new JsonRpcClient(dojoConfig.rpcUrl);
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
            
            await TryCreateLocalAccount(3, false);
            IncomingModelsFilter.SetLocalPlayerId(LocalBurnerAccount.Address.Hex());
            
        }

        public async Task CreateBot()
        {
            LocalBot = await GetBotForGame(false);
            if (LocalBot == null)
            {
                CustomLogger.LogError("Failed to create bot");
            }
        }

        public async Task SyncInitialModels()
        {
            int count = 0;
            await CustomSynchronizationMaster.SyncGeneralModels();
            await CustomSynchronizationMaster.SyncPlayer(LocalBurnerAccount.Address);
            await CustomSynchronizationMaster.SyncPlayerInProgressGame(LocalBurnerAccount.Address);
            // if player has an in progress game, sync the board with dependencies
            // TODO: remove logic duplication. It's already in SessionManager.Start
            bool hasGame = await CheckGameInProgress();
            if (hasGame)
            {
                await SyncEverythingForGame();
                //RestoreGame(board);
            }
            else
            {
                //LoadMenu();
            }
        }

        public async Task SyncEverythingForGame()
        {
            CustomLogger.LogDojoLoop("SyncEverythingForGame");
            await CustomSynchronizationMaster.SyncPlayerInProgressGame(LocalBurnerAccount.Address);
            evolute_duel_Game game = WorldManager.Entities<evolute_duel_Game>().FirstOrDefault()?.GetComponent<evolute_duel_Game>();
            FieldElement boardId = game.board_id switch
            {
                Option<FieldElement>.Some some => some.value,
                _ => null
            };
            IncomingModelsFilter.SetSessionCurrentBoardId(boardId.Hex());
            CustomLogger.LogDojoLoop("SyncEverythingForGame. BoardId: " + boardId.Hex());
            int count = await CustomSynchronizationMaster.SyncBoardWithDependencies(boardId);
            CustomLogger.LogInfo("Board synced: " + WorldManager.Entities<evolute_duel_Board>().Length);
            evolute_duel_Board board = WorldManager.Entities<evolute_duel_Board>().FirstOrDefault()?.GetComponent<evolute_duel_Board>();
            FieldElement[] players = new FieldElement[] { board.player1.Item1, board.player2.Item1 };
            IncomingModelsFilter.SetSessionPlayers(players.Select(p => p.Hex()).ToList());
            await CustomSynchronizationMaster.SyncPlayersArray(players);
            var allowedBoards = await CustomSynchronizationMaster.SyncAllMoveByBoardId(board.id);
            if (allowedBoards != null)
                IncomingModelsFilter.SetSessionAllowedBoards(allowedBoards.ToList());
            
            
            CustomLogger.LogDojoLoop("SyncEverythingForGame. Synced boards: " + count);
        }
        
        private async Task<bool> CheckGameInProgress()
        {
            int boardCount = 0;
            GameObject[] games = WorldManager.Entities<evolute_duel_Game>();
            if (games.Length > 0)
            {
                evolute_duel_Game game = games[0].GetComponent<evolute_duel_Game>();
                var player = await CustomSynchronizationMaster.WaitForModelByPredicate<evolute_duel_Player>(
                    p => p.player_id.Hex() == game.player.Hex()
                );
                
                if (player == null)
                {
                    CustomLogger.LogError("Failed to load player model for game");
                    return false;
                }

                FieldElement boardId = game.board_id switch
                {
                    Option<FieldElement>.Some some => some.value,
                    _ => null
                };
                
                boardCount = await CustomSynchronizationMaster.SyncBoardWithDependencies(boardId);
            }
            return boardCount > 0;
        }

        public void LoadGame()
        {
            GameObject boardObject = WorldManager.Entities<evolute_duel_Board>().FirstOrDefault();
            CustomLogger.LogInfo($"LoadGame. Board object: {boardObject}");
            // it's mean that player has an in progress game, so load the session
            if (boardObject != null)
                RestoreGame();
            else
                LoadMenu();
            
        }

        private void RestoreGame()
        {
            SessionManager = new DojoSessionManager(this);
            CustomSceneManager.Instance.LoadSession(
                startAction: () =>
                    CustomSceneManager.Instance.LoadingScreen.SetActive(true, 
                        () => DojoConnector.CancelGame(DojoGameManager.Instance.LocalBurnerAccount), 
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
            int count = await CustomSynchronizationMaster.SyncPlayerSnapshots(LocalBurnerAccount.Address);
        }
        
        #region Account Creation 
        private async Task TryCreateLocalAccount(int attempts, bool createNew)
        {
            try
            {
                for (int i = 0; i < attempts; i++)
                {
                    CustomLogger.LogInfo($"Creating burner account. Attempt: {i}");
                    if (await CreateLocalAccount(createNew))
                    {
                        CustomLogger.LogInfo($"Burner account created. Attempt: {i}. Address: {LocalBurnerAccount.Address.Hex()}");
                        OnLocalPlayerSet?.Invoke();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Failed to create burner account. {e}");
            }
        }
        
        
        public async Task<bool> CreateLocalAccount(bool createNew)
        {
            try
            {
                if (createNew)
                {
                    LocalBurnerAccount = await burnerManager.DeployBurner();
                }
                else
                {
                    string storedAccountAddress = SimpleStorage.LoadPlayerAddress();
                    bool isBurnersEmpty = burnerManager.Burners.Count == 0;
                    Account storedAccount = burnerManager.Burners.FirstOrDefault(b => b.Address.Hex() == storedAccountAddress);
                    LocalBurnerAccount = storedAccount;
                    if (storedAccount == null || isBurnersEmpty)
                    {
                        CustomLogger.LogWarning("Burner account not found. Creating new account.");
                        LocalBurnerAccount = await burnerManager.DeployBurner();
                    }
                }
                SimpleStorage.SetPlayerAddress(LocalBurnerAccount.Address.Hex());
                return true;
            }
            catch (Exception e)
            {
                return false;
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
                foreach (var burner in burnerManager.Burners)
                {
                    CustomLogger.LogWarning($"Burner address: {burner.Address.Hex()} Target address: {address}");
                }
                CustomLogger.LogError("Failed to get burner account");
                return false;
            }
            return true;
            
        }
        #endregion

        public async void CreateGameWithBots()
        {
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
            await DojoConnector.CreateGame(LocalBurnerAccount);
            CustomLogger.LogDojoLoop("Game created");
            DojoConnector.JoinGame(LocalBot.Account, LocalBurnerAccount.Address);
            CustomLogger.LogDojoLoop("Bot joined game");
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
            bot.Initialize(account);
            DojoConnector.BecameBot(account);
            return bot;
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
            CustomLogger.LogImportant($"Received event: {modelInstance.Model.Name}");
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
            CustomLogger.LogWarning("Burner account address:" + LocalBurnerAccount.Address.Hex());
            CustomLogger.LogWarning("Real caller address from event: " + eventMessage.player_id.Hex());
            if(LocalBurnerAccount == null || LocalBurnerAccount.Address.Hex() != eventMessage.player_id.Hex()) return;
            MenuUIController.Instance._namePanelController.SetName(CairoFieldsConverter.GetStringFromFieldElement(eventMessage.new_username));
        }
        
        private void GameCreateFailed(evolute_duel_GameCreateFailed eventMessage)
        {
            string hostPlayer = eventMessage.host_player.Hex();
            if (LocalBurnerAccount.Address.Hex() != hostPlayer) return;
            DojoConnector.CancelGame(LocalBurnerAccount);
            InvokeWithDelay(() => DojoConnector.CreateGame(LocalBurnerAccount), 1f);
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
                                 $"\nLocalPlayerIsHost: {gameStarted.host_player.Hex() == LocalBurnerAccount.Address.Hex()}" +
                                 $"\nLocalPlayerIsGuest: {gameStarted.guest_player.Hex() == LocalBurnerAccount.Address.Hex()}" +
                                 $"\nEventHostPlayerAddress: {gameStarted.host_player.Hex()}" +
                                 $"\nEventGuestPlayerAddress: {gameStarted.guest_player.Hex()}" +
                                 $"\nLocalPlayerAddress: {LocalBurnerAccount.Address.Hex()}");
            if (gameStarted.host_player.Hex() == LocalBurnerAccount.Address.Hex() ||
                gameStarted.guest_player.Hex() == LocalBurnerAccount.Address.Hex())
            {
                CustomLogger.LogInfo("Start session");
                // Start session
                ApplicationState.SetState(ApplicationStates.Initializing);
                await SyncEverythingForGame();
                SessionManager = new DojoSessionManager(this);
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
            evolute_duel_Player player = GetPlayerData(LocalBurnerAccount.Address.Hex());
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
        }
    }
}