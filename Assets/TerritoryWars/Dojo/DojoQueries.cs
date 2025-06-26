using System;
using System.Linq;
using dojo_bindings;
using Dojo;
using Dojo.Starknet;
using Dojo.Torii;
using TerritoryWars.General;
using TerritoryWars.Tools;

namespace TerritoryWars.Dojo
{
    public static class DojoQueries
    {
        public static uint limit = 1000;
        public static uint offset = 0;
        public static bool dont_include_hashed_keys = false;
        public static ulong entity_updated_after = 0;
        
        /// <summary>
        /// Gets a query to fetch a single player by their address
        /// </summary>
        /// <param name="address">The player's address as FieldElement</param>
        /// <returns>Query object configured to fetch player data</returns>
        public static Query GetQueryPlayer(FieldElement address)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Player>() };
            var memberClause = new MemberClause(
                GetModelName<evolute_duel_Player>(),
                "player_id",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = address })
                );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, memberClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        /// <summary>
        /// Gets a query to fetch multiple players by their addresses
        /// </summary>
        /// <param name="addresses">Array of player addresses as FieldElements</param>
        /// <returns>Query object configured to fetch multiple players' data</returns>
        public static Query GetQueryPlayersArray(FieldElement[] addresses)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Player>() };

            var addressValues = addresses.Select(addr =>
                new MemberValue(new Primitive { Felt252 = addr })
            ).ToArray();
            
            var memberClause = new MemberClause(
                GetModelName<evolute_duel_Player>(),
                "player_id",
                dojo.ComparisonOperator.In, 
                new MemberValue(addressValues) 
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, memberClause, dont_include_hashed_keys, entity_models);
            return query;
        }

        public static Query GetQueryTopPlayersForLeaderboard(uint count, uint playersBalance)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Player>() };
            
            var notBotClause = new MemberClause(
                GetModelName<evolute_duel_Player>(),
                "role",
                dojo.ComparisonOperator.Neq,
                new MemberValue(new Primitive { U8 = 2 })
            );

            var balanceClause = new MemberClause(
                GetModelName<evolute_duel_Player>(),
                "balance",
                dojo.ComparisonOperator.Gte,
                new MemberValue(new Primitive { U8 = (byte)playersBalance })
            );
            
            OrderBy[] order_by = new[]
            {
                new OrderBy(
                    model: GetModelName<evolute_duel_Player>(),
                    member: "balance",
                    direction: dojo.OrderDirection.Desc)
            };
            var compositeClause = new CompositeClause(
                dojo.LogicalOperator.And,
                new[] { (Clause)notBotClause, balanceClause }
            );
            
            Pagination pagination = new Pagination(limit: count, order_by: order_by);
            Query query = new Query(pagination, notBotClause, dont_include_hashed_keys, entity_models);
            return query;
        }

        /// <summary>
        /// Gets a query to fetch games in progress for a specific player
        /// </summary>
        /// <param name="address">The player's address as FieldElement</param>
        /// <returns>Query object configured to fetch in-progress games for the player</returns>
        public static Query GetQueryPlayerInProgressGame(FieldElement address)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Game>() };
            
            var playerClause = new MemberClause(
                GetModelName<evolute_duel_Game>(),
                "player",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = address })
                );

            var statusClause = new MemberClause(
                GetModelName<evolute_duel_Game>(),
                "status",
                dojo.ComparisonOperator.Eq,
                new MemberValue("InProgress")
                );
            
            var compositeClause = new CompositeClause(
                dojo.LogicalOperator.And,
            new[] { (Clause)playerClause, (Clause)statusClause }
                );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, compositeClause, dont_include_hashed_keys, entity_models);
            return query;
        }

        public static Query GetQueryAllGameInProgress()
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Game>() };

            var statusClause = new MemberClause(
                GetModelName<evolute_duel_Game>(),
                "status",
                dojo.ComparisonOperator.Eq,
                new MemberValue("InProgress")
            );
            
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, statusClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        public static Query GetQueryGameByBoardId(FieldElement boardId)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Game>() };
            
            var boardIdClause = new MemberClause(
                GetModelName<evolute_duel_Game>(),
                "board_id",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = boardId })
            );
            
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, boardIdClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        /// <summary>
        /// Gets a query to fetch general game models including Rules and Shop
        /// </summary>
        /// <returns>Query object configured to fetch general game models</returns>
        public static Query GetGeneralModels()
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Rules>(), GetModelName<evolute_duel_Shop>() };
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, null, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        /// <summary>
        /// Gets a query to fetch a specific game board by its ID
        /// </summary>
        /// <param name="id">The board ID as FieldElement</param>
        /// <returns>Query object configured to fetch board data</returns>
        public static Query GetQueryOnlyBoard(FieldElement id)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Board>() };
            var memberClause = new MemberClause(
                GetModelName<evolute_duel_Board>(),
                "id",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, memberClause, dont_include_hashed_keys, entity_models);
            return query;
        }

        public static Query GetQueryUnionFind(FieldElement id)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_UnionFind>() };
            var memberClause = new MemberClause(
                GetModelName<evolute_duel_UnionFind>(),
                "board_id",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, memberClause, dont_include_hashed_keys, entity_models);
            return query;
        }

        /// <summary>
        /// Gets a query to fetch a game board and all its dependencies (city nodes, road nodes)
        /// </summary>
        /// <param name="id">The board ID as FieldElement</param>
        /// <returns>Query object configured to fetch board and related data</returns>
        public static Query GetQueryBoardWithDependencies(FieldElement id)
        {
            string[] entity_models = new[]
            {
                GetModelName<evolute_duel_Board>(), 
                GetModelName<evolute_duel_CityNode>(),
                GetModelName<evolute_duel_RoadNode>(),
            };
            var boardIdClause = new MemberClause(
                GetModelName<evolute_duel_Board>(),
                "id",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            
            var moveClause = new MemberClause( 
                GetModelName<evolute_duel_Move>(),
                "first_board_id", 
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            
            var cityNodeClause = new MemberClause(
                GetModelName<evolute_duel_CityNode>(),
                "board_id",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            
            var roadNodeClause = new MemberClause(
                GetModelName<evolute_duel_RoadNode>(),
                "board_id",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            
            var compositeClause = new CompositeClause(
                dojo.LogicalOperator.Or,
                new[] { (Clause)boardIdClause, cityNodeClause, roadNodeClause }
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, compositeClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        /// <summary>
        /// Gets a query to fetch a move by its first board ID
        /// </summary>
        /// <param name="id">The board ID as FieldElement</param>
        /// <returns></returns>
        public static Query GetQueryMoveByFirstBoardId(FieldElement id)
        {
            string[] entity_models = new[]
            {
                GetModelName<evolute_duel_Move>(), 
            };
            
            var moveClause = new MemberClause( 
                GetModelName<evolute_duel_Move>(),
                "first_board_id", 
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, moveClause, dont_include_hashed_keys, entity_models);
            return query;
        }

        public static Query GetQueryMoveById(FieldElement id)
        {
            string[] entity_models = new[]
            {
                GetModelName<evolute_duel_Move>(), 
            };
            
            var moveClause = new MemberClause( 
                GetModelName<evolute_duel_Move>(),
                "id", 
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = id })
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, moveClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        public static Query GetQueryMoveByIdArray(FieldElement[] ids)
        {
            string[] entity_models = new[]
            {
                GetModelName<evolute_duel_Move>(), 
            };
            
            var idValues = ids.Select(id => 
                new MemberValue(new Primitive { Felt252 = id })
            ).ToArray();
            
            var memberClause = new MemberClause(
                GetModelName<evolute_duel_Move>(),
                "id",
                dojo.ComparisonOperator.In, 
                new MemberValue(idValues) 
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, memberClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        /// <summary>
        /// Gets a query to fetch all games with 'Created' status
        /// </summary>
        /// <returns>Query object configured to fetch created games</returns>
        public static Query GetQueryCreatedGame()
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Game>() };

            var statusClause = new MemberClause(
                GetModelName<evolute_duel_Game>(),
                "status",
                dojo.ComparisonOperator.Eq,
                new MemberValue("Created")
            );
            
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, statusClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        /// <summary>
        /// Gets a query to fetch all snapshots for a specific player
        /// </summary>
        /// <param name="playerAddress">The player's address as FieldElement</param>
        /// <returns>Query object configured to fetch player snapshots</returns>
        public static Query GetQueryPlayerSnapshots(FieldElement playerAddress)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Snapshot>() };

            var statusClause = new MemberClause(
                GetModelName<evolute_duel_Snapshot>(),
                "player",
                dojo.ComparisonOperator.Eq,
                new MemberValue(new Primitive { Felt252 = playerAddress })
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, statusClause, dont_include_hashed_keys, entity_models);
            return query;
        }
        
        public static Query GetQuerySnapshotArray(FieldElement[] snapshotIds)
        {
            string[] entity_models = new[] { GetModelName<evolute_duel_Snapshot>() };
            
            var snapshotValues = snapshotIds.Select(id => 
                new MemberValue(new Primitive { Felt252 = id })
            ).ToArray();
            
            var memberClause = new MemberClause(
                GetModelName<evolute_duel_Snapshot>(),
                "snapshot_id",
                dojo.ComparisonOperator.In, 
                new MemberValue(snapshotValues) 
            );
            Pagination pagination = new Pagination(limit);
            Query query = new Query(pagination, memberClause, dont_include_hashed_keys, entity_models);
            return query;
        }

        /// <summary>
        /// Gets the formatted model name for a given type
        /// </summary>
        /// <typeparam name="T">The model type that inherits from ModelInstance</typeparam>
        /// <returns>Formatted string containing the namespace and model name</returns>
        public static string GetModelName<T>() where T : ModelInstance
        {
            string @namespace = "evolute_duel";
            string name = typeof(T).Name;
            return $"{@namespace}-{name.Replace("evolute_duel_", "")}";
        }
    }
}