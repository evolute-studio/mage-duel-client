using System;
using System.Collections.Generic;
using System.Linq;
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
        public string TopTile;
        public Dictionary<Vector2Int, Tile> Tiles; // key: (x, y) position, value: Tile struct
        public SessionPlayer Player1;
        public SessionPlayer Player2;
        public string LastMoveId; // Maybe better store Move struct
        public BoardState GameState;
        
        public BoardUpdated SetData(evolute_duel_BoardUpdated boardUpdated)
        {
            Id = boardUpdated.board_id.Hex();
            AvailableTilesInDeck = boardUpdated.available_tiles_in_deck.ToList()
                .Select(x => GameConfiguration.GetTileType(x)).ToArray();
            TopTile = GameConfiguration.GetTileType(boardUpdated.top_tile.Unwrap());
            Tiles = new Dictionary<Vector2Int, Tile>();
            for(int i = 0; i < boardUpdated.state.Length; i++)
            {
                var tile = boardUpdated.state[i];
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
            GameState = (BoardState)boardUpdated.game_state.Unwrap();
            return this;
        }
    }
}