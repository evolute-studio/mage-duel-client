using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dojo.Starknet;
using TerritoryWars.Contracts;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.ExternalConnections
{
    public static class DojoConnector
    {
        public static Game GameContract => DojoGameManager.Instance.GameSystem;

        public static Player_profile_actions PlayerProfileActionsContract =>
            DojoGameManager.Instance.PlayerProfileSystem;
        
        // private async void CreateGameAction()
        // {
        //     CustomSceneManager.Instance.LoadingScreen.SetActive(true, CancelGame, LoadingScreen.waitAnathorPlayerText);
        //     var txHash = await GameSystem.create_game(LocalBurnerAccount);
        //     CustomLogger.LogInfo($"Create Game: {txHash.Hex()}");
        // }
        
        #region Game Actions
        public static async Task CreateGame(GeneralAccount account)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithLoading(LoadingScreen.waitAnotherPlayerText, () => CancelGame(account))
                .WithMessage($"DojoCall: [{nameof(CreateGame)}] " +
                             $"\n Account: {account.Address.Hex()}");
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.create_game(), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.create_game(account.Account),
                    executeConfig
                );
            }
            
        }
        
        public static async void CreateGameFromSnapshot(GeneralAccount account, FieldElement snapshotId)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithLoading(LoadingScreen.waitAnotherPlayerText, () => CancelGame(account))
                .WithMessage($"DojoCall: [{nameof(CreateGameFromSnapshot)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n SnapshotId: {snapshotId.Hex()}");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.create_game_from_snapshot(snapshotId), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.create_game_from_snapshot(account.Account, snapshotId),
                    executeConfig
                );
            }
            
            
        }
        
        // this method also have to use SessionManager = new DojoSessionManager(this); but I gonna refactor DojoSessionManager first
        public static async void JoinGame(GeneralAccount account, FieldElement hostPlayer)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithLoading(LoadingScreen.connectingText, () => CancelGame(account))
                .WithMessage($"DojoCall: [{nameof(JoinGame)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n HostPlayer: {hostPlayer.Hex()}");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.join_game(hostPlayer), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.join_game(account.Account, hostPlayer),
                    executeConfig
                );
            }
        }
        
        private static void ReturnToMenu()
        {
            if (CustomSceneManager.Instance.CurrentScene != CustomSceneManager.Instance.Menu)
                CustomSceneManager.Instance.LoadLobby();
        }
        
        public static async void CancelGame(GeneralAccount account)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(CancelGame)}] " +
                             $"\n Account: {account.Address.Hex()}")
                .OnFailure(ReturnToMenu)
                .OnSuccess(() =>
                {
                    CustomSceneManager.Instance.LoadingScreen.SetActive(false);
                    ReturnToMenu();
                });
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.cancel_game(), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.cancel_game(account.Account),
                    executeConfig
                );
            }
        }
        
        public static async void FinishGame(GeneralAccount account, FieldElement boardId)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(FinishGame)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n BoardId: {boardId.Hex()}");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.finish_game(boardId), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.finish_game(account.Account, boardId),
                    executeConfig
                );
            }
        }
        
        public static async void MakeMove(GeneralAccount account, Option<byte> joker_tile, byte rotation, byte col, byte row)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(MakeMove)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n JokerTile: {joker_tile.Unwrap()} Rotation: {rotation} Col: {col} Row: {row}");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.make_move(joker_tile, rotation, col, row), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.make_move(account.Account, joker_tile, rotation, col, row),
                    executeConfig
                );
            }
        }

        public static async void CreateSnapshot(GeneralAccount account, FieldElement boardId, byte moveNumber)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(CreateSnapshot)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n BoardId: {boardId.Hex()} MoveNumber: {moveNumber}");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.create_snapshot(boardId, moveNumber), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.create_snapshot(account.Account, boardId, moveNumber),
                    executeConfig
                );
            }
        }

        public static async void SkipMove(GeneralAccount account)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(SkipMove)}] " +
                             $"\n Account: {account.Address.Hex()}");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.skip_move(), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => GameContract.skip_move(account.Account),
                    executeConfig
                );
            }
        }
        #endregion

        
        #region Player Profile Actions
        public static async Task ChangeUsername(GeneralAccount account, FieldElement name)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(ChangeUsername)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n Name: { CairoFieldsConverter.GetStringFromFieldElement(name)}");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.change_username(name), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => PlayerProfileActionsContract.change_username(account.Account, name),
                    executeConfig
                );
            }
        }
        
        public static async void ChangeSkin(GeneralAccount account, int skinId)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(ChangeSkin)}]");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.change_skin(skinId), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => PlayerProfileActionsContract.change_skin(account.Account, (byte)skinId),
                    executeConfig
                );
            }
        }
        
        public static async void BecameBot(GeneralAccount account)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(BecameBot)}]");
            
            if (account.IsController)
            {
                ExecuteController(ControllerContracts.become_bot(), executeConfig);
            }
            else
            {
                await TryExecuteAction(
                    account.Account,
                    () => PlayerProfileActionsContract.become_bot(account.Account),
                    executeConfig
                );
            }
        }
        
        #endregion
        
        
        #region Helpers
        private static async Task<bool> TryExecuteAction(Account account, Func<Task<FieldElement>> action, ExecuteConfig config = null)
        {
            config ??= new ExecuteConfig();
            config.OnStartAction?.Invoke();
            try
            {
                if (config.WithLoadingScreen)
                    CustomSceneManager.Instance.LoadingScreen.SetActive(true, config.CancelAction, config.LoadingText);
                
                var txHash = await action();
                
                if(!String.IsNullOrEmpty(config.Message))
                    CustomLogger.LogExecution(config.Message + " success");
                config.OnSuccessAction?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                if(config.WithLoadingScreen)
                    CustomSceneManager.Instance.LoadingScreen.SetActive(false);
    
                if(!String.IsNullOrEmpty(config.Message))
                    CustomLogger.LogError(config.Message + " failed. Error: ", e);
                config.OnFailureAction?.Invoke();

                if (e.Message.Contains("ContractNotFound"))
                {
                    ContractNotFoundHandler(account);
                }
                
                return false;
            }
        }

        private static void ExecuteController(string transaction, ExecuteConfig config = null)
        {
            config ??= new ExecuteConfig();
            config.OnStartAction?.Invoke();
            if (config.WithLoadingScreen)
                CustomSceneManager.Instance.LoadingScreen.SetActive(true, config.CancelAction, config.LoadingText);
            if (!String.IsNullOrEmpty(config.Message))
                CustomLogger.LogExecution(config.Message + " success");
            WrapperConnectorCalls.ExecuteController(transaction);
            config.OnSuccessAction?.Invoke();
        }
        
        #endregion

        private static async void ContractNotFoundHandler(Account account)
        {
            string playerAddress = SimpleStorage.LoadPlayerAddress();
            string botAddress = SimpleStorage.LoadBotAddress();
            
            if (account.Address.Hex() == playerAddress)
            {
                PlayerContractNotFoundHandler();
            }
            else if (account.Address.Hex() == botAddress)
            {
                BotContractNotFoundHandler();
            }
        }
        
        private static async void PlayerContractNotFoundHandler()
        {
            CustomLogger.LogError("The Player contract was not found. Maybe a problem in creating an account");
            await DojoGameManager.Instance.CreateAccount(true);
            CustomSceneManager.Instance.LoadLobby();
        }
        
        private static async void BotContractNotFoundHandler()
        {
            CustomLogger.LogError("The Bot contract was not found. Maybe a problem in creating an account");
            CancelGame(DojoGameManager.Instance.LocalAccount);
            await DojoGameManager.Instance.GetBotForGame(true);
            CustomSceneManager.Instance.LoadLobby();
        }
    }
    
    public class ExecuteConfig 
    {
        public bool WithLoadingScreen;
        public string LoadingText;
        public Action CancelAction;

        public string Message;
        
        public Action OnStartAction;
        public Action OnSuccessAction;
        public Action OnFailureAction;
        
        public Dictionary<string, Action> OnErrorActions;

        public ExecuteConfig(string message = "")
        {
            WithLoadingScreen = false;
            LoadingText = "";
            CancelAction = null;
            Message = message;
            OnStartAction = null;
            OnSuccessAction = null;
            OnFailureAction = null;
        }
        
        public ExecuteConfig WithLoading(string loadingText = null, Action cancelAction = null)
        {
            WithLoadingScreen = true;
            LoadingText = loadingText;
            CancelAction = cancelAction;
            return this;
        }
        
        public ExecuteConfig WithActions(Action onStart = null, Action onSuccess = null, Action onFailure = null)
        {
            OnStartAction = onStart;
            OnSuccessAction = onSuccess;
            OnFailureAction = onFailure;
            return this;
        }
        
        public ExecuteConfig WithMessage(string message)
        {
            Message = message;
            return this;
        }
        
        public ExecuteConfig OnStart(Action onStart)
        {
            OnStartAction = onStart;
            return this;
        }
        
        public ExecuteConfig OnSuccess(Action onSuccess)
        {
            OnSuccessAction = onSuccess;
            return this;
        }
        
        public ExecuteConfig OnFailure(Action onFailure)
        {
            OnFailureAction = onFailure;
            return this;
        }
        
        public ExecuteConfig OnErrorAction(string error, Action action)
        {
            if (OnErrorActions == null)
                OnErrorActions = new Dictionary<string, Action>();
            OnErrorActions.Add(error, action);
            return this;
        }
        
        public Action GetErrorAction(string error)
        {
            if (OnErrorActions == null)
                return null;
            return OnErrorActions.ContainsKey(error) ? OnErrorActions[error] : null;
        }
    }
        
}