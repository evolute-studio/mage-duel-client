using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dojo.Starknet;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tools;

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
        public static async Task CreateGame(Account account)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithLoading(LoadingScreen.waitAnotherPlayerText, () => CancelGame(account))
                .WithMessage($"DojoCall: [{nameof(CreateGame)}] " +
                             $"\n Account: {account.Address.Hex()}");
            await TryExecuteAction(
                account,
                () => GameContract.create_game(account),
                executeConfig
            );
        }
        
        public static async void CreateGameFromSnapshot(Account account, FieldElement snapshotId)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithLoading(LoadingScreen.waitAnotherPlayerText, () => CancelGame(account))
                .WithMessage($"DojoCall: [{nameof(CreateGameFromSnapshot)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n SnapshotId: {snapshotId.Hex()}");
            await TryExecuteAction(
                account,
                () => GameContract.create_game_from_snapshot(account, snapshotId),
                executeConfig
            );
        }
        
        // this method also have to use SessionManager = new DojoSessionManager(this); but I gonna refactor DojoSessionManager first
        public static async void JoinGame(Account account, FieldElement hostPlayer)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithLoading(LoadingScreen.connectingText, () => CancelGame(account))
                .WithMessage($"DojoCall: [{nameof(JoinGame)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n HostPlayer: {hostPlayer.Hex()}");
            await TryExecuteAction(
                account,
                () => GameContract.join_game(account, hostPlayer),
                executeConfig
            );
        }
        
        public static async void CancelGame(Account account)
        {
            void ReturnToMenu()
            {
                if (CustomSceneManager.Instance.CurrentScene != CustomSceneManager.Instance.Menu)
                    CustomSceneManager.Instance.LoadLobby();
            }
            
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(CancelGame)}] " +
                             $"\n Account: {account.Address.Hex()}")
                .OnFailure(ReturnToMenu)
                .OnSuccess(() =>
                {
                    CustomSceneManager.Instance.LoadingScreen.SetActive(false);
                    ReturnToMenu();
                });
                
            await TryExecuteAction(
                account,
                () => GameContract.cancel_game(account),
                executeConfig
            );
        }
        
        public static async void MakeMove(Account account, Option<byte> joker_tile, byte rotation, byte col, byte row)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(MakeMove)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n JokerTile: {joker_tile.Unwrap()} Rotation: {rotation} Col: {col} Row: {row}");
            await TryExecuteAction(
                account,
                () => GameContract.make_move(account, joker_tile, rotation, col, row),
                executeConfig
            );
        }

        public static async void CreateSnapshot(Account account, FieldElement boardId, byte moveNumber)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(CreateSnapshot)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n BoardId: {boardId.Hex()} MoveNumber: {moveNumber}");
            await TryExecuteAction(
                account,
                () => GameContract.create_snapshot(account, boardId, moveNumber),
                executeConfig
            );
        }

        public static async void SkipMove(Account account)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(SkipMove)}] " +
                             $"\n Account: {account.Address.Hex()}");
            await TryExecuteAction(
                account,
                () => GameContract.skip_move(account),
                executeConfig
            );
        }
        #endregion

        
        #region Player Profile Actions
        public static async Task ChangeUsername(Account account, FieldElement name)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(ChangeUsername)}] " +
                             $"\n Account: {account.Address.Hex()} " +
                             $"\n Name: { CairoFieldsConverter.GetStringFromFieldElement(name)}");
            await TryExecuteAction(
                account,
                () => PlayerProfileActionsContract.change_username(account, name),
                executeConfig
            );
        }
        
        public static async void ChangeSkin(Account account, int skinId)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(ChangeSkin)}]");
            await TryExecuteAction(
                account,
                () => PlayerProfileActionsContract.change_skin(account, (byte)skinId),
                executeConfig
            );
        }
        
        public static async void BecameBot(Account account)
        {
            ExecuteConfig executeConfig = new ExecuteConfig()
                .WithMessage($"DojoCall: [{nameof(BecameBot)}]");
            await TryExecuteAction(
                account,
                () => PlayerProfileActionsContract.become_bot(account),
                executeConfig
            );
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
                    CustomLogger.LogError(config.Message + " failed. Error: " + e.Message);
                config.OnFailureAction?.Invoke();

                if (e.Message.Contains("ContractNotFound"))
                {
                    ContractNotFoundHandler(account);
                }
                
                return false;
            }
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
            await DojoGameManager.Instance.CreateLocalAccount(true);
            CustomSceneManager.Instance.LoadLobby();
        }
        
        private static async void BotContractNotFoundHandler()
        {
            CustomLogger.LogError("The Bot contract was not found. Maybe a problem in creating an account");
            CancelGame(DojoGameManager.Instance.LocalBurnerAccount);
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