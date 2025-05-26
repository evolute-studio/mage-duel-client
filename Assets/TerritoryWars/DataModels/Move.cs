using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct Move
    {
        public string Id;
        public byte PlayerSide;
        public string PrevMoveId;
        public Tile Tile;
        public bool IsJoker;
        public string FirstBoardId;
        public ulong Timestamp;
        
        public bool IsSkip()
        {
            return String.IsNullOrEmpty(Tile.Type);
        }

        public Move SetData(evolute_duel_Move move)
        {
            Id = move.id.Hex();
            PlayerSide = (byte)move.player_side.Unwrap();
            PrevMoveId = move.prev_move_id.Unwrap()?.Hex();
            Tile = new Tile()
            {
                Type = GameConfiguration.GetTileType(move.tile.Unwrap()),
                Position = GameConfiguration.GetClientPosition(move.col, move.row),
                Rotation = GameConfiguration.GetClientRotation(move.rotation),
                PlayerSide = PlayerSide
            };
            IsJoker = move.is_joker;
            FirstBoardId = move.first_board_id.Hex();
            Timestamp = move.timestamp;
            return this;
        }
    }
}