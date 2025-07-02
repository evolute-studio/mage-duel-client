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
        public Board BoardForLoad; // used just for loading board, not used in game
        public SessionContext SessionContext;
        public bool LastCreatedGameWithBot = false;
        
        public bool HasGameInProgress => !GameInProgress.IsNull;
    }
}