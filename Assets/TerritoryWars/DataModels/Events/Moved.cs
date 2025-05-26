using System;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct Moved
    {
        public bool IsNull => String.IsNullOrEmpty(Id);
        
        public string Id;
        public string PlayerId;
        public string PrevMoveId;
        public Tile Tile;
        public bool IsJoker;
        public string BoardId;
        public string FirstBoardId;
        public ulong Timestamp;
        
        public bool IsSkip()
        {
            return String.IsNullOrEmpty(Tile.Type);
        }

        public Moved SetData(evolute_duel_Moved moved)
        {
            Id = moved.move_id.Hex();
            PlayerId = moved.player.Hex();
            PrevMoveId = moved.prev_move_id.Unwrap()?.Hex();
            Tile = new Tile()
            {
                Type = GameConfiguration.GetTileType(moved.tile.Unwrap()),
                Position = GameConfiguration.GetClientPosition(moved.col, moved.row),
                Rotation = GameConfiguration.GetClientRotation(moved.rotation),
                //PlayerSide = PlayerSide
            };
            IsJoker = moved.is_joker;
            BoardId = moved.board_id.Hex();
            Timestamp = moved.timestamp;
            return this;
        }
    }
}