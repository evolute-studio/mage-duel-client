using System;
using TerritoryWars.DataModels;
using TerritoryWars.Managers.SessionComponents;

namespace TerritoryWars.Managers
{
    [Serializable]
    public class GlobalContext
    {
        public Rules Rules;
        public Shop Shop;
        public PlayerProfile PlayerProfile;
        public GameModel GameInProgress;
        public Board Board;
        public SessionContext SessionContext;


        public bool IsJustLoadedSession = false;
        public bool HasGameInProgress => !GameInProgress.IsNull;
    }
}