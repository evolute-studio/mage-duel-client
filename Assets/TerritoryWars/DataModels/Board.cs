using System;
using System.Collections.Generic;
using System.Linq;
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
        public Dictionary<Vector2Int, Tile> Tiles; // key: (x, y) position, value: Tile struct
        public SessionPlayer Player1;
        public SessionPlayer Player2;
        public string LastMoveId; // Maybe better store Move struct
        public BoardState GameState;
        
        public string OldBoardId; // For snapshot
        public ushort MoveCount;
        
        public Board SetData(evolute_duel_Board board)
        {
            Id = board.id.Hex();
            InitialEdgeState = GameConfiguration.GetInitialEdgeState(board.initial_edge_state);
            AvailableTilesInDeck = board.available_tiles_in_deck.ToList()
                .Select(x => GameConfiguration.GetTileType(x)).ToArray();
            TopTile = GameConfiguration.GetTileType(board.top_tile.Unwrap());
            Tiles = new Dictionary<Vector2Int, Tile>();
            AddEdgeTiles(Tiles, board.initial_edge_state);
            for(int i = 0; i < board.state.Length; i++)
            {
                var tile = board.state[i];
                var position = GameConfiguration.GetClientPosition(i);
                Tiles[position] = new Tile()
                {
                    Type = GameConfiguration.GetTileType(tile.Item1),
                    Position = position,
                    Rotation = GameConfiguration.GetClientRotation(tile.Item2),
                    PlayerSide = tile.Item3
                };
            }

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
            return this;
        }
        
        
        public static void AddEdgeTiles(Dictionary<Vector2Int,Tile> dict, byte[] edgeState)
        {
            FillCorners(dict);
            
            int countPerSide = edgeState.Length / 4;
            char[] tiles = GameConfiguration.GetInitialEdgeState(edgeState);
            
            // i: 0 edgeState[0] = 'C' type = "CFFF" rotation = 3, playerSide = 3, position = (1, 0)
            // i: 8 edgeState[8] = 'R' type = "RFFF" rotation = 2, playerSide = 3
            // i: 16 edgeState[16] = 'F' type = "FFFF" rotation = 1, playerSide = 3
            // i: 24 edgeState[24] = 'M' type = "MFFF" rotation = 0, playerSide = 3
            for(int i = 0; i < edgeState.Length; i++)
            {
                char edgeType = tiles[i];
                // from 0 to 8 rotation = 3
                // from 8 to 16 rotation = 2
                // from 16 to 24 rotation = 1
                // from 24 to 32 rotation = 0
                int rotation = (3 - (i / countPerSide)) % 4; 
                Vector2Int position;
                switch (i / countPerSide)
                {
                    case 0: 
                        position = new Vector2Int(i % countPerSide + 1, 0);
                        break;
                    case 1: 
                        position = new Vector2Int(GameConfiguration.ClientBoardSize.x - 1, i % countPerSide + 1);
                        break;
                    case 2: 
                        position = new Vector2Int(GameConfiguration.ClientBoardSize.x - (i % countPerSide) - 2, GameConfiguration.ClientBoardSize.y - 1);
                        break;
                    case 3: 
                        position = new Vector2Int(0, GameConfiguration.ClientBoardSize.y - (i % countPerSide) - 2);
                        break;
                    default:
                        continue; // Should not happen
                }
                
                if (!dict.ContainsKey(position))
                {
                    dict.Add(position, new Tile()
                    {
                        Type = $"{edgeType}FFF", // Default empty tile with edge type
                        Position = position,
                        Rotation = (byte)rotation,
                        PlayerSide = 3 // No player side
                    });
                }
                
                
            }
        }

        public static void FillCorners(Dictionary<Vector2Int,Tile> dict)
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
                    dict.Add(position, new Tile()
                    {
                        Type = "FFFF", // Default empty tile
                        Position = position,
                        Rotation = 0,
                        PlayerSide = 3 // No player side
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