using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.Dojo
{
    public static class DojoModels
    {
        public static WorldManager WorldManager => DojoLayer.Instance.WorldManager;
        public static CustomSynchronizationMaster SynchronizationMaster => DojoLayer.Instance.SynchronizationMaster;
        
        public static async Task<(Rules, Shop)> GetGeneralModels()
        {
            Rules rulesModel = new Rules();
            Shop shopModel = new Shop();
            evolute_duel_Rules rules = WorldManager.EntityModel<evolute_duel_Rules>();
            evolute_duel_Shop shop = WorldManager.EntityModel<evolute_duel_Shop>();
            if (rules == null || shop == null)
            {
                await SynchronizationMaster.SyncGeneralModels();
                rules = WorldManager.EntityModel<evolute_duel_Rules>();
                shop = WorldManager.EntityModel<evolute_duel_Shop>();
            }

            if (rules == null || shop == null)
            {
                return (default, default);
            }
            rulesModel.SetData(rules);
            shopModel.SetData(shop);
            return (rulesModel, shopModel);
        }

        public static async Task<Board> GetBoard(string boardId)
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

        public static async Task<UnionFind> GetUnionFind(string boardId)
        {
            evolute_duel_UnionFind unionFind = WorldManager.EntityModel<evolute_duel_UnionFind>("board_id", new FieldElement(boardId));
            if (unionFind == null)
            {
                await SynchronizationMaster.SyncUnionFind(new FieldElement(boardId));
                unionFind = WorldManager.EntityModel<evolute_duel_UnionFind>("board_id", new FieldElement(boardId));
            }
            if (unionFind == null)
            {
                return default;
            }

            UnionFind unionFindModel = new UnionFind().SetData(unionFind);
            return unionFindModel;
        }

        public static async Task<Move> GetMove(string moveId)
        {
            evolute_duel_Move move = WorldManager.EntityModel<evolute_duel_Move>("id", new FieldElement(moveId));
            if (move == null)
            {
                await SynchronizationMaster.SyncMoveById(new FieldElement(moveId));
                move = WorldManager.EntityModel<evolute_duel_Move>("id", new FieldElement(moveId));
            }
            if (move == null)
            {
                return default;
            }

            Move boardModel = new Move().SetData(move);
            return boardModel;

        }

        public static async Task<Move> GetMoves(string moveId)
        {
            evolute_duel_Move move = WorldManager.EntityModel<evolute_duel_Move>("id", new FieldElement(moveId));
            if (move == null)
            {
                await SynchronizationMaster.SyncMoveById(new FieldElement(moveId));
                move = WorldManager.EntityModel<evolute_duel_Move>("id", new FieldElement(moveId));
            }
            if (move == null)
            {
                return default;
            }

            Move boardModel = new Move().SetData(move);
            return boardModel;
        }

        public static List<Move> GetMoves(List<Move> moves, GameObject[] allMoveGameObjects = null)
        {
            if (allMoveGameObjects == null)
            {
                allMoveGameObjects = WorldManager.Entities<evolute_duel_Move>();
            }

            Move currentMove = moves.First();
            string previousMoveId = currentMove.PrevMoveId;
            if (String.IsNullOrEmpty(previousMoveId))
            {
                return moves;
            }

            foreach (var moveGO in allMoveGameObjects)
            {
                if (moveGO.TryGetComponent(out evolute_duel_Move move))
                {
                    if (move.id.Hex() == previousMoveId)
                    {
                        Move moveData = new Move().SetData(move);
                        moves.Insert(0, moveData);
                        return GetMoves(moves, allMoveGameObjects);
                    }
                }
            }

            return moves;
        }

        public static async Task<PlayerProfile> GetPlayerProfile(string playerId)
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

        public static async Task<GameModel> GetGameInProgress(string playerId)
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
        
        public static async Task<GameModel> GetGameByBoardId(string boardId)
        {
            Dictionary<string, object> filters = new Dictionary<string, object>()
            {
                { "board_id", new FieldElement(boardId) }
            };
            evolute_duel_Game game = WorldManager.EntityModel<evolute_duel_Game>(filters);
            if (game == null)
            {
                await SynchronizationMaster.SyncGameByBoardId(new FieldElement(boardId));
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