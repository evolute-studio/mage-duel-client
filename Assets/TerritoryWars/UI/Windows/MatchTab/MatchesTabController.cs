using System;
using System.Collections.Generic;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.ConnectorLayers.WebSocketLayer;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.WebSocketEvents;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.SaveStorage;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI.Windows.MatchTab
{
    public class MatchesTabController : NetworkWindow
    {
        [Header("Additional References", order = 0)]
        [SerializeField] private Button createMatchButton;
        [SerializeField] private Button createBotMatchButton;
        [SerializeField] private CanvasGroup itemsParentCanvasGroup;
        
        private int _createdMatchesCount = 0;

        // public GameObject PanelGameObject;
        // public GameObject MatchListItemPrefab;
        // public Transform ListItemParent;
        // public GameObject BackgroundPlaceholderGO;
        
        // public CanvasGroup canvasGroup;
        // public GameObject CloseButtonGO;
        //private List<MatchListItem> _matchListItems = new List<MatchListItem>();

        // General Window Methods
        public void Start() => Initialize();
        
        public override void Initialize()
        {
            base.Initialize();
            createMatchButton.onClick.AddListener(CreateMatch);
            createBotMatchButton.onClick.AddListener(CreateMatchWithBot);
        }

        protected override void PanelActiveTrue()
        {
            base.PanelActiveTrue();
            ApplicationState.SetState(ApplicationStates.MatchTab);
            SetActiveItems(false);
            Invoke(nameof(ActivatePanel), 2f);
            FetchData();
            DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
            EventBus.Subscribe<WebSocketClient.OnlinePlayers>(OnOnlinePlayers);
            
            void ActivatePanel()
            {
                SetActiveItems(true);
            }
        }

        protected override void PanelActiveFalse()
        {
            base.PanelActiveFalse();
            ApplicationState.SetState(ApplicationStates.Menu);
            DojoGameManager.Instance.CustomSynchronizationMaster.DestroyPlayersExceptLocal(DojoGameManager.Instance.LocalAccount.Address);
            DojoGameManager.Instance.CustomSynchronizationMaster.DestroyAllGames();
            DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnEventMessage);
            EventBus.Unsubscribe<WebSocketClient.OnlinePlayers>(OnOnlinePlayers);
        }
        
        // Network Window Methods
        protected override void OnEventMessage(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                case evolute_duel_GameCreated:
                    FetchData();
                    break;
                case evolute_duel_GameCanceled gameCanceled:
                    CancelGame(gameCanceled);
                    break;
            }
        }

        private void OnOnlinePlayers(WebSocketClient.OnlinePlayers players)
        {
            Dictionary<string, bool> onlinePlayers = players.ToDictionary();
            CustomLogger.LogImportant($"[WebSocketClient] Online players received: {onlinePlayers.Count}");
            foreach (MatchListItem matchListItem in listItems)
            {
                if (onlinePlayers.TryGetValue(matchListItem.HostPlayer, out bool isOnline))
                {
                    matchListItem.OnlineStatus.SetOnline(isOnline);
                }
            }
            listItems.Sort((a, b) => 
                ((MatchListItem)a).IsOnline.CompareTo(((MatchListItem)b).IsOnline) * -1);
            
            for (int i = 0; i < listItems.Count; i++)
            {
                (listItems[i] as MatchListItem)?.ListItem.transform.SetSiblingIndex(i);
            }
            
            SetActiveItems(true);
        }

        private void SetActiveItems(bool active)
        {
            itemsParentCanvasGroup.alpha = active ? 1f : 0f;
        }

        private void GetOnline(List<string> players)
        {
            WebSocketClient.CheckOnline(players);
        }

        protected override async void FetchData()
        {
            try
            {
                await DojoGameManager.Instance.SyncCreatedGames();
                GameObject[] games = DojoGameManager.Instance.GetGames();
                evolute_duel_Game[] gameModels = new evolute_duel_Game[games.Length];
                List<string> players = new List<string>();
                
                for (int i = 0; i < games.Length; i++)
                {
                    if (!games[i].TryGetComponent(out evolute_duel_Game gameModel))
                    {
                        CustomLogger.LogError("MatchesTabController. Game model not found on game object");
                        continue;
                    }
                    gameModels[i] = gameModel;
                    players.Add(gameModel.player.Hex());
                }
                
                GetOnline(players);

                foreach (var gameModel in gameModels)
                {
                    PlayerProfile player = await DojoModels.GetPlayerProfile(gameModel.player.Hex());
                    if(IsMatchListItemExists(player.PlayerId)) continue;
                    
                    string playerName = player.Username;
                    uint evoluteBalance = player.Balance;
                    FieldElement snapshotId = gameModel.snapshot_id switch
                    {
                        Option<FieldElement>.Some some => some.value,
                        Option<FieldElement>.None => null
                    };
                    evolute_duel_Snapshot snapshotModel = DojoGameManager.Instance.GetSnapshot(snapshotId);
                    int moveNumber = snapshotModel != null ? snapshotModel.move_number : 0;
                    string status = gameModel.status switch
                    {
                        GameStatus.Created => "Created",
                        GameStatus.InProgress => "In Progress",
                        GameStatus.Finished => "Finished",
                        GameStatus.Canceled => "Canceled",
                        _ => "Unknown"
                    };
                    switch (status)
                    {
                        case "Created":
                            _createdMatchesCount++;
                            break;
                    }
                    MatchListItem matchListItem = CreateListItem<MatchListItem>();
                    if( status == "Created")
                    {
                        matchListItem.UpdateItem(playerName, evoluteBalance, status, player.PlayerId, moveNumber,() =>
                        {
                            SetActivePanel(false);
                            DojoConnector.JoinGame(DojoGameManager.Instance.LocalAccount, gameModel.player);
                            SimpleStorage.SetIsGameWithBot(false);
                        });
                    }
                    else
                    {
                        Destroy(matchListItem.ListItem);
                        listItems.Remove(matchListItem);
                    }
                }

                SetBackgroundPlaceholder(_createdMatchesCount == 0);
                SortItems();
            }
            catch (Exception e)
            {
                CustomLogger.LogError("MatchesTabController. Failed to fetch data", e);
            }
        }
        
        // Specific Methods
        
        private bool IsMatchListItemExists(string hostPlayer)
        {
            foreach (MatchListItem matchListItem in listItems)
            {
                if (matchListItem.HostPlayer == hostPlayer)
                {
                    return true;
                }
            }

            return false;
        }
        
        private void SortItems()
        {
            for (int i = 0; i < listItems.Count; i++)
            {
                (listItems[i] as MatchListItem)?.ListItem.transform.SetSiblingIndex(i);
            }
        }

        public void CreateMatch()
        {
            SetActivePanel(false);
            DojoConnector.CreateGame(DojoGameManager.Instance.LocalAccount);
            SimpleStorage.SetIsGameWithBot(false);
        }

        public void CreateMatchWithBot()
        {
            SetActivePanel(false);
            DojoGameManager.Instance.CreateGameWithBots();
            SimpleStorage.SetIsGameWithBot(true);
        }
        
        private void CancelGame(evolute_duel_GameCanceled gameCanceled)
        {
            foreach (MatchListItem matchListItem in listItems)
            {
                if (matchListItem.HostPlayer == gameCanceled.host_player.Hex())
                {
                    Destroy(matchListItem.ListItem);
                    listItems.Remove(matchListItem);
                    break;
                }
            }
        }
        
        
    }
}