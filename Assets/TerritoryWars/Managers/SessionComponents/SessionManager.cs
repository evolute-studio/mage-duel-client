using System;
using System.Collections.Generic;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class SessionManager : MonoBehaviour
    {
        public SessionContext SessionContext = new SessionContext();
        private SessionManagerContext _managerContext;
        private List<ISessionComponent> _components;

        private void Start()
        {
            SetupData();
            Initialize();
        }

        private void Initialize()
        {
            _managerContext = new SessionManagerContext();
            _components = new List<ISessionComponent>();

            var playersManager = new PlayersManager();
            var gameLoopManager = new GameLoopManager();

            _managerContext.SessionContext = SessionContext;
            _managerContext.SessionManager = this;
            _managerContext.PlayersManager = playersManager;
            _managerContext.GameLoopManager = gameLoopManager;

            _components.Add(playersManager);
            _components.Add(gameLoopManager);

            foreach (var component in _components)
                component.Initialize(_managerContext);
        }
        
        public async void SetupData()
        {
            SessionContext.LocalPlayerAddress = DojoGameManager.Instance.LocalAccount.Address.Hex();
            GameModel game = await DojoLayer.Instance.GetGameInProgress(SessionContext.LocalPlayerAddress);
            if (game.IsNull)
            {
                CustomLogger.LogError("[SessionManager.SetupData] - Game is null");
                CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                return;
            }
            SessionContext.Game = game;
            Board board = await DojoLayer.Instance.GetBoard(SessionContext.Game.BoardId);
            if (board.IsNull)
            {
                CustomLogger.LogError("[SessionManager.SetupData] - Board is null");
                CustomSceneManager.Instance.ForceLoadScene(CustomSceneManager.Instance.Menu);
                return;
            }
            SessionContext.Board = board;
            SessionContext.Players[0] = board.Player1;
            SessionContext.Players[1] = board.Player2;
            PlayerProfile player1 = await DojoLayer.Instance.GetPlayerProfile(board.Player1.PlayerId);
            PlayerProfile player2 = await DojoLayer.Instance.GetPlayerProfile(board.Player2.PlayerId);
            SessionContext.Players[0].ActiveSkin = player1.ActiveSkin;
            SessionContext.Players[1].ActiveSkin = player2.ActiveSkin;
        }
        
        private void OnDestroy()
        {
            foreach (var component in _components)
                component.Dispose();
        }
    }
}