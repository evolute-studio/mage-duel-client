using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.Dojo;
using TerritoryWars.Tools;
using UnityEngine;
using System.Threading.Tasks;
using TerritoryWars.ConnectorLayers;
using TerritoryWars.Contracts;
using TerritoryWars.ExternalConnections;
using TerritoryWars.SaveStorage;

namespace TerritoryWars.General
{
    public enum GameMode
    {
        Offline,
        OnChain
    }
    
    public enum ConnectionType
    {
        Local,
        RemoteDev,
        RemoteProd,
        None
    }

    public class EntryPoint : MonoBehaviour
    {
        public static EntryPoint Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public WorldManager WorldManager;
        public DojoGameManager DojoGameManager;
        public DojoGameController DojoGameGUIController;
        public bool UseDojoGUIController = false;

        public WrapperConnectorCalls.ConnectionData connection;
        public Game game_contract;
        public Player_profile_actions player_profile_actions;

        private float startConenctionTime;
        public WorldManagerData dojoConfig;
        

    public void Start()
    {
            SimpleStorage.Initialize();
            #if !UNITY_EDITOR && UNITY_WEBGL
            connection = WrapperConnectorCalls.GetConnectionData();
            //CustomLogger.LogImportant($"[Connection] rpcUrl {connection.rpcUrl} toriiUrl {connection.toriiUrl} gameAddress {connection.gameAddress} playerProfileActionsAddress {connection.playerProfileActionsAddress}");
            ControllerContracts.EVOLUTE_DUEL_GAME_ADDRESS = connection.gameAddress;
            ControllerContracts.EVOLUTE_DUEL_PLAYER_PROFILE_ACTIONS_ADDRESS = connection.playerProfileActionsAddress;
            #else
            connection.SetData(dojoConfig);
            #endif
            game_contract.contractAddress = connection.gameAddress;
            player_profile_actions.contractAddress = connection.playerProfileActionsAddress;
            string worldAddress = connection.worldAddress;
            DojoGameManager.Instance.WorldManager.Initialize(connection.rpcUrl, connection.toriiUrl, worldAddress);
            
            CustomLogger.LogDojoLoop($"[Connection data] " +
                                     $"rpcUrl {connection.rpcUrl} " +
                                     $"toriiUrl {connection.toriiUrl} " +
                                     $"gameAddress {connection.gameAddress} " +
                                     $"playerProfileActionsAddress {connection.playerProfileActionsAddress}" +
                                     $"worldAddress {connection.worldAddress}" +
                                     $"slotDataVersion {connection.slotDataVersion}");
            
            CustomLogger.LogDojoLoop("Starting Loading Game");
            CustomSceneManager.Instance.LoadingScreen.SetActive(true, null, LoadingScreen.launchGameText);
            bool isControllerLogged = WrapperConnectorCalls.IsControllerLoggedIn();
            if(isControllerLogged)
            {
                CustomLogger.LogDojoLoop("Controller is logged in");
                ControllerLogin();
            }
            else
            {
                CustomLogger.LogDojoLoop("Controller is not logged in");
                CustomSceneManager.Instance.LoadingScreen.SetActive(false);
            }
        }
        
        public async void ControllerLogin()
        {
            ApplicationState.IsController = true;
            WrapperConnectorCalls.ControllerLogin();
        }

        public async void GuestLogin()
        {
            ApplicationState.IsController = false;
            CustomSceneManager.Instance.LoadingScreen.SetActive(true, null, LoadingScreen.launchGameText);
            await InitializeGuestGameAsync();
        }
        
        public async Task InitializeControllerGameAsync()
        {
            if (ApplicationState.IsLoggedIn)
            {
                CustomLogger.LogDojoLoop("Already logged in");
                return;
            }
            ApplicationState.IsLoggedIn = true;
            ApplicationState.IsController = true;
            
            InitDataStorage();
            
            try
            {
                CustomLogger.LogDojoLoop("Starting OnChain mode initialization");
                
                // 1. Setup Account
                CustomLogger.LogDojoLoop("Setting up account");
                await SetupAccountAsync(connection);
                
                // 2. Create Burners
                CustomLogger.LogDojoLoop("Creating burner accounts");
                await DojoGameManager.CreateBurners();

                CustomLogger.LogDojoLoop("Creating local controller player");
                FieldElement controllerAddress = new FieldElement(WrapperConnector.instance.address);
                DojoGameManager.SetLocalControllerAccount(controllerAddress);
                //
                // await CoroutineAsync(() => { }, 2f);
                //
                CustomLogger.LogDojoLoop("Creating bot");
                await DojoGameManager.CreateBot();
                
                // 3. Sync Initial Models
                CustomLogger.LogDojoLoop("Syncing initial models");
                await DojoGameManager.SyncInitialModels();
                
                // 4. Load Game
                CustomLogger.LogDojoLoop("Checking previous game");
                DojoGameManager.LoadGame();
                
                CustomLogger.LogDojoLoop("Initialization completed successfully");
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Initialization failed:", e);
            }
        }
        
        public async Task InitializeGuestGameAsync()
        {
            InitDataStorage();
            
            try
            {
                CustomLogger.LogDojoLoop("Starting OnChain mode initialization");
                
                // 1. Setup Account
                CustomLogger.LogDojoLoop("Setting up account");
                await SetupAccountAsync(connection);
                
                // 2. Create Burners
                CustomLogger.LogDojoLoop("Creating burner accounts");
                await DojoGameManager.CreateBurners();

                CustomLogger.LogDojoLoop("Creating local player");
                await DojoGameManager.CreateLocalPlayer();
                //
                // await CoroutineAsync(() => { }, 2f);
                //
                CustomLogger.LogDojoLoop("Creating bot");
                await DojoGameManager.CreateBot();
                
                // 3. Sync Initial Models
                CustomLogger.LogDojoLoop("Syncing initial models");
                await DojoGameManager.SyncInitialModels();
                
                // 4. Load Game
                CustomLogger.LogDojoLoop("Checking previous game");
                DojoGameManager.LoadGame();
                
                CustomLogger.LogDojoLoop("Initialization completed successfully");
            }
            catch (Exception e)
            {
                CustomLogger.LogError($"Initialization failed:", e);
            }
        }

        private Task SetupAccountAsync(WrapperConnectorCalls.ConnectionData connection = default)
        {
            var tcs = new TaskCompletionSource<bool>();
            
            try {
                DojoGameManager.SetupMasterAccount(() => tcs.TrySetResult(true), connection);
                // timeout to avoid hanging
                StartCoroutine(SetupAccountTimeout(tcs, 30f));
            }
            catch (Exception e) {
                tcs.TrySetException(e);
            }
            
            return tcs.Task;
        }
        
        private IEnumerator SetupAccountTimeout(TaskCompletionSource<bool> tcs, float timeout)
        {
            yield return new WaitForSeconds(timeout);
            tcs.TrySetException(new TimeoutException($"Account setup timed out after {timeout} seconds"));
        }
        
        private async Task CoroutineAsync(Action action, float delay = 0f)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(WaitForCoroutine(tcs, action, delay));
            await tcs.Task;
        }
        
        private IEnumerator WaitForCoroutine(TaskCompletionSource<bool> tcs, Action action, float delay = 0f)
        {
            yield return new WaitForSeconds(delay);
            action();
            tcs.TrySetResult(true);
        }
        

        private void InitDataStorage()
        {
            int dataVersion = SimpleStorage.LoadDataVersion();
            if (dataVersion < connection.slotDataVersion)
            {
                CustomLogger.LogDojoLoop($"A new version of slot data has been detected. Local: {dataVersion}, Remote: {connection.slotDataVersion}");
                SimpleStorage.ClearAll();
                SimpleStorage.SetDataVersion(connection.slotDataVersion);
            }
            
        }
    }


}