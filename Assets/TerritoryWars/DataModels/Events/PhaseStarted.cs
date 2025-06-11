using System;
using UnityEngine;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct PhaseStarted
    {
        public string BoardId;
        public SessionPhase Phase;
        public string TopTile;
        public byte? CommitedTileIndex;
        public ulong StartedAt;

        public PhaseStarted SetData(evolute_duel_PhaseStarted phaseStarted)
        {
            BoardId = phaseStarted.board_id.ToString();
            Phase = (SessionPhase)phaseStarted.phase;
            byte? top_tile = phaseStarted.top_tile.UnwrapByte();
            TopTile = top_tile.HasValue ? GameConfiguration.GetTileType(top_tile.Value) : null;
            CommitedTileIndex = phaseStarted.commited_tile.UnwrapByte();
            StartedAt = phaseStarted.started_at;
            return this;
        }
        
        public PhaseStarted SetData(Board board)
        {
            BoardId = board.Id;
            Phase = board.Phase;
            TopTile = board.TopTile;
            CommitedTileIndex = board.CommitedTileIndex;
            StartedAt = board.LastUpdateTimestamp;
            return this;
        }
    }
    
    

    public enum SessionPhase
    {
        Creating = 0,
        Reveal = 1,
        Request = 2,
        Move = 3,
        Finished = 4
    }
}