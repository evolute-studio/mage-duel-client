using System;
using TerritoryWars.DataModels;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    [Serializable]
    public class SessionContext
    {
        public string LocalPlayerAddress;
        public GameModel Game;
        public Board Board;
        public SessionPlayer[] Players = new SessionPlayer[2];
        
        public Vector3[] SpawnPoints; 

        public Player LocalPlayer;
        
        public Player RemotePlayer;
        public Player CurrentTurnPlayer;
        //public PlayerProfile[] PlayersProfiles = new PlayerProfile[2];
    }
}