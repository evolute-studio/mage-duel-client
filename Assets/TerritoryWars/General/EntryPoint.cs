using System;
using System.Collections;
using System.Collections.Generic;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.Dojo;
using TerritoryWars.Tools;
using UnityEngine;
using System.Threading.Tasks;
using TerritoryWars.ExternalConnections;

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
        
        private float startConenctionTime;

        public void ControllerLogin()
        {
            ApplicationState.IsController = true;
            WrapperConnectorTest.ControllerLogin();
        }

        public async void GuestLogin()
        {
            ApplicationState.IsController = false;
            CustomSceneManager.Instance.LoadingScreen.SetActive(true, null, LoadingScreen.launchGameText);
            await InitializeGameAsync();
        }
        
        public async Task InitializeGameAsync()
        {
            InitDataStorage();
            
            try
            {
                CustomLogger.LogDojoLoop("Starting OnChain mode initialization");
                
                // 1. Setup Account
                CustomLogger.LogDojoLoop("Setting up account");
                await SetupAccountAsync();
                
                // 2. Create Burners
                CustomLogger.LogDojoLoop("Creating burner accounts");
                await DojoGameManager.CreateBurners();
                
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

        private Task SetupAccountAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            
            try {
                DojoGameManager.SetupMasterAccount(() => tcs.TrySetResult(true));
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
        

        
        
        private void SceneLoaded(string name)
        {
            if (name == CustomSceneManager.Instance.Menu)
            {
                
            }
            else if (name == CustomSceneManager.Instance.Session)
            {
                //SessionManager.Instance.Initialize();

            }
        }

        private void InitDataStorage()
        {
            int currentDataVersion = 14;
            int dataVersion = SimpleStorage.LoadDataVersion();
            if (dataVersion < currentDataVersion)
            {
                SimpleStorage.ClearAll();
                SimpleStorage.SetDataVersion(currentDataVersion);
            }
            
        }
        
        private void OnDestroy()
        {
            CustomSceneManager.Instance.OnLoadScene -= SceneLoaded;
        }
    }


}