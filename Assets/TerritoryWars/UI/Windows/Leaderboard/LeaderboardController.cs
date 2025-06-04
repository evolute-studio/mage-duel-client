using System;
using System.Collections.Generic;
using DG.Tweening;
using Dojo;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI.Windows.Leaderboard
{
    public class LeaderboardController : NetworkWindow
    {
        [SerializeField] private uint _playerToShow = 20;
        [SerializeField] private uint _playerBalanceToShow = 100;
        [SerializeField] private uint _leaderPlaceToShow = 3;

        // General Window Methods
        public void Start() => Initialize();
        
        protected override void PanelActiveTrue()
        {
            base.PanelActiveTrue();
            FetchData();
        }
        
        // Network Window Methods
        protected override async void FetchData()
        {
            ApplicationState.SetState(ApplicationStates.Leaderboard);
            int count =
                await DojoGameManager.Instance.CustomSynchronizationMaster.SyncTopPlayersForLeaderboard(_playerToShow, _playerBalanceToShow);
            ApplicationState.SetState(ApplicationStates.Menu);
            GameObject[] playersGO = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Player>();
            List<evolute_duel_Player> players = new List<evolute_duel_Player>();
            foreach (var player in playersGO)
            {
                players.Add(player.GetComponent<evolute_duel_Player>());
            }

            players.Sort((x, y) => y.balance.CompareTo(x.balance));
            evolute_duel_Player localPlayer = DojoGameManager.Instance.GetLocalPlayerData();
            for (int i = (int)_playerToShow; i < players.Count; i++)
            {
                if (players[i].player_id.Hex() == localPlayer.player_id.Hex())
                    continue;
                IncomingModelsFilter.DestroyModel(players[i]);
            }


            for (int i = 0; i < _playerToShow; i++)
            {
                if (i >= players.Count)
                    break;
                string playerName = CairoFieldsConverter.GetStringFromFieldElement(players[i].username);
                
                if (String.IsNullOrEmpty(playerName) || playerName.StartsWith("Bot") || players[i].balance <= 0)
                {
                    i = i == 0 ? 0 : i--;
                    continue;
                }

                LeaderboardListItem leaderboardItem = CreateListItem<LeaderboardListItem>();
                string name = CairoFieldsConverter.GetStringFromFieldElement(players[i].username);
                string address = players[i].player_id.Hex();
                uint balance = players[i].balance;
                leaderboardItem.UpdateItem(name, balance, address);
                leaderboardItem.SetLeaderPlace(i + 1, _leaderPlaceToShow);
                
            }
            
            SetBackgroundPlaceholder(listItems.Count == 0);
        }
    }
}