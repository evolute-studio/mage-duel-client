using System;
using TerritoryWars.DataModels;
using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    [Serializable]
    public class SessionContext
    {
        public string LocalPlayerAddress;
        public GameModel Game;
        public Board Board;
        public UnionFind UnionFind;
        public SessionPlayer[] PlayersData = new SessionPlayer[2];

        public ulong LastUpdateTimestamp => Board.LastUpdateTimestamp;


        [Header("Players")]
        public Player[] Players;
        public Player LocalPlayer;
        public Player RemotePlayer;
        public Player CurrentTurnPlayer;

        public bool IsLocalPlayerHost => LocalPlayer.PlayerId == Board.Player1.PlayerId;
        public bool IsLocalPlayerTurn => CurrentTurnPlayer.PlayerId == LocalPlayer.PlayerId;

        public bool IsGameWithBot;
        public bool IsGameWithBotAsPlayer;


        [Header("Session Settings")]
        public Vector3[] SpawnPoints;

        public bool IsSessionBoard(string boardId)
        {
            return Board.Id == boardId;
        }

        public bool IsSessionMove(string moveId, string boardId)
        {
            return Board.Id == boardId || Board.LastMoveId == moveId;
        }


        public bool IsPlayerInSession(string playerId)
        {
            return PlayersData[0].PlayerId == playerId || PlayersData[1].PlayerId == playerId;
        }

        public Player GetPlayerById(string playerId)
        {
            if (Players[0].PlayerId == playerId)
            {
                return Players[0];
            }
            if (Players[1].PlayerId == playerId)
            {
                return Players[1];
            }
            return null;
        }
        //public PlayerProfile[] PlayersProfiles = new PlayerProfile[2];
    }
}
