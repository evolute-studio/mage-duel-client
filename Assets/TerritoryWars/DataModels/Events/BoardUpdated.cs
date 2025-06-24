using System;
using System.Collections.Generic;
using System.Linq;
using Dojo.Starknet;
using UnityEngine;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct BoardUpdated
    {
        public bool IsNull => String.IsNullOrEmpty(Id);
        
        public string Id;
        //public ushort MoveCount;
        public string[] AvailableTilesInDeck;
        public byte? TopTileIndex;
        public string TopTile => AvailableTilesInDeck[TopTileIndex ?? 0];
        
        public Dictionary<Vector2Int, TileModel> Tiles; // key: (x, y) position, value: Tile struct
        public SessionPlayer Player1;
        public SessionPlayer Player2;
        public string LastMoveId; // Maybe better store Move struct
        public SessionPhase GameState;
        
        public BoardUpdated SetData(evolute_duel_BoardUpdated boardUpdated)
        {
            Id = boardUpdated.board_id.Hex();
            AvailableTilesInDeck = boardUpdated.available_tiles_in_deck.ToList()
                .Select(x => GameConfiguration.GetTileType(x)).ToArray();
            TopTileIndex = boardUpdated.top_tile.UnwrapByte();
            Tiles = new Dictionary<Vector2Int, TileModel>();
            for(int i = 0; i < boardUpdated.state.Length; i++)
            {
                var tile = boardUpdated.state[i];
                var position = GameConfiguration.GetClientPosition(i);
                Tiles[position] = new TileModel()
                {
                    Type = GameConfiguration.GetTileType(tile.Item1),
                    Position = position,
                    Rotation = GameConfiguration.GetClientRotation(tile.Item2),
                    PlayerSide = tile.Item3
                };
            }

            Player1 = new SessionPlayer()
            {
                PlayerId = boardUpdated.player1.Item1.Hex(),
                PlayerSide = (byte)boardUpdated.player1.Item2.Unwrap(),
                JokerCount = boardUpdated.player1.Item3,
                Score = new SessionPlayerScore()
                {
                    CityScore = boardUpdated.blue_score.Item1,
                    RoadScore = boardUpdated.blue_score.Item2
                }
            };
            Player2 = new SessionPlayer()
            {
                PlayerId = boardUpdated.player2.Item1.Hex(),
                PlayerSide = (byte)boardUpdated.player2.Item2.Unwrap(),
                JokerCount = boardUpdated.player2.Item3,
                Score = new SessionPlayerScore()
                {
                    CityScore = boardUpdated.red_score.Item1,
                    RoadScore = boardUpdated.red_score.Item2
                }
            };
            LastMoveId = boardUpdated.last_move_id.Unwrap()?.Hex();
            GameState = boardUpdated.game_state.Unwrap();
            return this;
        }

        public BoardUpdated SetData(Board board)
        {
            Id = board.Id;
            AvailableTilesInDeck = board.AvailableTilesInDeck;
            TopTileIndex = board.TopTileIndex;
            Tiles = board.Tiles;
            Player1 = board.Player1;
            Player2 = board.Player2;
            LastMoveId = board.LastMoveId;
            GameState = board.Phase;
            return this;
            
        }
    }
}