using System;
using TerritoryWars.DataModels;

namespace TerritoryWars.Managers
{
    [Serializable]
    public class GlobalContext
    {
        public Rules Rules;
        public Shop Shop;
        public PlayerProfile PlayerProfile;
        public GameModel GameInProgress;

        public bool HasGameInProgress => !GameInProgress.IsNull;
    }
}