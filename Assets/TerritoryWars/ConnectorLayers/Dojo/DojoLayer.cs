using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.Dojo
{
    public class DojoLayer: MonoBehaviour
    {
        public static DojoLayer Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public WorldManager WorldManager;
        public CustomSynchronizationMaster SynchronizationMaster;
        
        public EventsHandler EventsHandler;
        public string LocalPlayerId => DojoGameManager.Instance.LocalAccount.Address.Hex();

        public DojoLayer(WorldManager worldManager, CustomSynchronizationMaster synchronizationMaster)
        {
            WorldManager = worldManager;
            SynchronizationMaster = synchronizationMaster;

            EventsHandler = new EventsHandler(WorldManager);
        }

        public async Task<Board> GetBoard(string boardId)
        {
            evolute_duel_Board board = WorldManager.EntityModel<evolute_duel_Board>("id", new FieldElement(boardId));
            if (board == null)
            {
                await SynchronizationMaster.SyncOnlyBoard(new FieldElement(boardId));
                board = WorldManager.EntityModel<evolute_duel_Board>("id", new FieldElement(boardId));
            }
            if (board == null)
            {
                return default;
            }

            Board boardModel = new Board().SetData(board);
            return boardModel;
        }

        public async Task<PlayerProfile> GetPlayerProfile(string playerId)
        {
            evolute_duel_Player player = WorldManager.EntityModel<evolute_duel_Player>("player_id", new FieldElement(playerId));
            if (player == null)
            {
                await SynchronizationMaster.SyncPlayer(new FieldElement(playerId));
                player = WorldManager.EntityModel<evolute_duel_Player>("player_id", new FieldElement(playerId));
            }
            if (player == null)
            {
                return default;
            }
            PlayerProfile profile = new PlayerProfile().SetData(player);
            return profile;
        }

        public async Task<GameModel> GetGameInProgress(string playerId)
        {
            Dictionary<string, object> filters = new Dictionary<string, object>()
            {
                { "player", new FieldElement(playerId) },
                { "status", new GameStatus.InProgress() }
            };
            evolute_duel_Game game = WorldManager.EntityModel<evolute_duel_Game>(filters);
            if (game == null)
            {
                await SynchronizationMaster.SyncPlayerInProgressGame(new FieldElement(playerId));
                game = WorldManager.EntityModel<evolute_duel_Game>(filters);
            }
            if (game == null)
            {
                return default;
            }
            GameModel gameModel = new GameModel().SetData(game);
            return gameModel;
        }

        

    }
}