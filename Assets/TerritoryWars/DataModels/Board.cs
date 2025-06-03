using System;
using System.Collections.Generic;
using System.Linq;
using TerritoryWars.DataModels.Events;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct Board
    {
        public bool IsNull => String.IsNullOrEmpty(Id);

        public string Id;
        public char[] InitialEdgeState;
        public string[] AvailableTilesInDeck;
        public string TopTile;
        public Dictionary<Vector2Int, TileModel> Tiles; // key: (x, y) position, value: Tile struct
        public SessionPlayer Player1;
        public SessionPlayer Player2;
        public string LastMoveId; // Maybe better store Move struct
        public BoardState GameState;
        public byte MovesDone;
        public int MovesCount => GetOnlyMovesCount();
        public ulong LastUpdateTimestamp;

        public Board SetData(evolute_duel_Board board)
        {
            Id = board.id.Hex();
            InitialEdgeState = GameConfiguration.GetInitialEdgeState(board.initial_edge_state);
            AvailableTilesInDeck = board.available_tiles_in_deck.ToList()
                .Select(x => GameConfiguration.GetTileType(x)).ToArray();
            TopTile = GameConfiguration.GetTileType(board.top_tile.Unwrap());
            Tiles = new Dictionary<Vector2Int, TileModel>();
            AddEdgeTiles(Tiles, board.initial_edge_state);
            for (int i = 0; i < board.state.Length; i++)
            {
                var tile = board.state[i];
                var position = GameConfiguration.GetClientPosition(i);
                Tiles[position] = new TileModel()
                {
                    Type = GameConfiguration.GetTileType(tile.Item1),
                    Position = position,
                    Rotation = GameConfiguration.GetClientRotation(tile.Item2),
                    PlayerSide = tile.Item3
                };
            }

            CheckEdgeConnections(Tiles);


            Player1 = new SessionPlayer()
            {
                PlayerId = board.player1.Item1.Hex(),
                PlayerSide = (byte)board.player1.Item2.Unwrap(),
                JokerCount = board.player1.Item3,
                Score = new SessionPlayerScore()
                {
                    CityScore = board.blue_score.Item1,
                    RoadScore = board.blue_score.Item2
                }
            };
            Player2 = new SessionPlayer()
            {
                PlayerId = board.player2.Item1.Hex(),
                PlayerSide = (byte)board.player2.Item2.Unwrap(),
                JokerCount = board.player2.Item3,
                Score = new SessionPlayerScore()
                {
                    CityScore = board.red_score.Item1,
                    RoadScore = board.red_score.Item2
                }
            };
            LastMoveId = board.last_move_id.Unwrap()?.Hex();
            GameState = (BoardState)board.game_state.Unwrap();
            MovesDone = board.moves_done;
            LastUpdateTimestamp = board.last_update_timestamp;
            return this;
        }

        public Board SetData(BoardUpdated data)
        {
            if (Id != data.Id)
            {
                CustomLogger.LogError($"[Board] | SetData: Board ID mismatch. Expected: {Id}, Received: {data.Id}");
                return this;
            }

            AvailableTilesInDeck = data.AvailableTilesInDeck;
            TopTile = data.TopTile;
            foreach (var eventTile in data.Tiles)
            {
                Tiles[eventTile.Key] = eventTile.Value;
            }
            Player1.Update(data.Player1);
            Player2.Update(data.Player2);
            LastMoveId = data.LastMoveId;
            GameState = data.GameState;

            return this;
        }
        
        public void UpdateTimestamp(ulong timestamp)
        {
            LastUpdateTimestamp = timestamp;
        }

        public int GetOnlyMovesCount()
        {
            // 32 tiles is border tiles, so we can skip them
            int count = -32;
            foreach (var tile in Tiles.Values)
            {
                if (tile.IsNull) continue;
                count++;
            }
            return count;
        }


        public static void AddEdgeTiles(Dictionary<Vector2Int, TileModel> dict, byte[] edgeState)
        {
            FillCorners(dict);

            int countPerSide = edgeState.Length / 4;
            char[] tiles = GameConfiguration.GetInitialEdgeState(edgeState);

            for (int i = 0; i < edgeState.Length; i++)
            {
                char edgeType = tiles[i];
                int rotation = (3 - (i / countPerSide)) % 4;
                Vector2Int position;
                int playerSide = -1; // Default no player side
                switch (i / countPerSide)
                {
                    case 0:
                        position = new Vector2Int(i % countPerSide + 1, 0);
                        break;
                    case 1:
                        position = new Vector2Int(GameConfiguration.ClientBoardSize.x - 1, i % countPerSide + 1);
                        break;
                    case 2:
                        position = new Vector2Int(GameConfiguration.ClientBoardSize.x - (i % countPerSide) - 2,
                            GameConfiguration.ClientBoardSize.y - 1);
                        break;
                    case 3:
                        position = new Vector2Int(0, GameConfiguration.ClientBoardSize.y - (i % countPerSide) - 2);
                        break;
                    default:
                        continue; // Should not happen
                }

                if (!dict.ContainsKey(position))
                {
                    string type = "";
                    switch (edgeType)
                    {
                        case 'M' or 'F':
                            type = "FFFF";
                            break;
                        case 'C':
                            type = "CFFF";
                            break;
                        case 'R':
                            type = "FFFR";
                            rotation = (rotation + 1) % 4; // Rotate to match the edge
                            break;
                    }

                    dict.Add(position, new TileModel()
                    {
                        Type = type,
                        Position = position,
                        Rotation = (byte)rotation,
                        PlayerSide = playerSide,
                    });
                }
            }
        }

        public static void CheckEdgeConnections(Dictionary<Vector2Int, TileModel> dict)
        {
            int countPerSide = GameConfiguration.ClientBoardSize.x - 2;
            int edgeCount = 4 * countPerSide;
            for (int i = 0; i < edgeCount; i++)
            {
                Vector2Int position;
                int playerSide = -1;
                switch (i / countPerSide)
                {
                    case 0:
                        position = new Vector2Int(i % countPerSide + 1, 0);
                        var neighbor = dict[new Vector2Int(position.x, position.y + 1)];
                        playerSide = neighbor.IsNull ? -1 : neighbor.PlayerSide;
                        break;
                    case 1:
                        position = new Vector2Int(GameConfiguration.ClientBoardSize.x - 1, i % countPerSide + 1);
                        neighbor = dict[new Vector2Int(position.x - 1, position.y)];
                        playerSide = neighbor.IsNull ? -1 : neighbor.PlayerSide;
                        break;
                    case 2:
                        position = new Vector2Int(GameConfiguration.ClientBoardSize.x - (i % countPerSide) - 2,
                            GameConfiguration.ClientBoardSize.y - 1);
                        neighbor = dict[new Vector2Int(position.x, position.y - 1)];
                        playerSide = neighbor.IsNull ? -1 : neighbor.PlayerSide;
                        break;
                    case 3:
                        position = new Vector2Int(0, GameConfiguration.ClientBoardSize.y - (i % countPerSide) - 2);
                        neighbor = dict[new Vector2Int(position.x + 1, position.y)];
                        playerSide = neighbor.IsNull ? -1 : neighbor.PlayerSide;
                        break;
                    default:
                        continue; // Should not happen
                }

                if (dict.ContainsKey(position))
                {
                    TileModel tileModel = dict[position];
                    tileModel.PlayerSide = playerSide;
                    dict[position] = tileModel;
                }
            }
        }

        public static bool IsEdgeTile(Vector2Int position)
        {
            return position.x == 0 || position.x == GameConfiguration.ClientBoardSize.x - 1 ||
                   position.y == 0 || position.y == GameConfiguration.ClientBoardSize.y - 1;
        }

        public static void FillCorners(Dictionary<Vector2Int, TileModel> dict)
        {
            Vector2Int[] emptyTilePositions = new Vector2Int[4]
            {
                new Vector2Int(0, 0),
                new Vector2Int(GameConfiguration.ClientBoardSize.x - 1, 0),
                new Vector2Int(GameConfiguration.ClientBoardSize.x - 1, GameConfiguration.ClientBoardSize.y - 1),
                new Vector2Int(0, GameConfiguration.ClientBoardSize.y - 1)
            };
            foreach (var position in emptyTilePositions)
            {
                if (!dict.ContainsKey(position))
                {
                    dict.Add(position, new TileModel()
                    {
                        Type = "FFFF", // Default empty tile
                        Position = position,
                        Rotation = 0,
                        PlayerSide = -1 // No player side
                    });
                }
            }
        }
    }

    public enum BoardState
    {
        InProgress,
        Finished
    }
}