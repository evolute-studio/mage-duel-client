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

namespace TerritoryWars.UI.Windows.Spectator
{
    public class SpectatorTabController : NetworkWindow
    {
        private int _gamesInProgress;

        public void Start() => Initialize();

        public override void Initialize()
        {
            base.Initialize();
        }
        
        protected override void PanelActiveTrue()
        {
            base.PanelActiveTrue();
            ApplicationState.SetState(ApplicationStates.SpectatorTab);
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
        
        protected override void OnEventMessage(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                case evolute_duel_GameStarted:
                    FetchData();
                    break;
                case evolute_duel_GameFinished gameFinished:
                    FinishGame(gameFinished);
                    break;
            }
        }

        protected override async void FetchData()
        {
            base.FetchData();
            try
            {
                await DojoGameManager.Instance.SyncCreatedGames();
                GameObject[] games = DojoGameManager.Instance.GetGames();



            }
            catch(Exception e)
            {
                CustomLogger.LogError("SpectatorController. Failed to fetch data", e);
            }
        }
        
        
        private void FinishGame(evolute_duel_GameFinished gameFinished)
        {
            
        }
    }
}