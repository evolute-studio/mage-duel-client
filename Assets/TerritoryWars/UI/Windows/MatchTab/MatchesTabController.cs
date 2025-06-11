using System;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
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
            FetchData();
            DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
        }

        protected override void PanelActiveFalse()
        {
            base.PanelActiveFalse();
            ApplicationState.SetState(ApplicationStates.Menu);
            DojoGameManager.Instance.CustomSynchronizationMaster.DestroyPlayersExceptLocal(DojoGameManager.Instance.LocalAccount.Address);
            DojoGameManager.Instance.CustomSynchronizationMaster.DestroyAllGames();
            DojoGameManager.Instance.WorldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnEventMessage);

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
        protected override async void FetchData()
        {
            try
            {
                await DojoGameManager.Instance.SyncCreatedGames();
                GameObject[] games = DojoGameManager.Instance.GetGames();

                foreach (var game in games)
                {
                    if (!game.TryGetComponent(out evolute_duel_Game gameModel)) return;
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