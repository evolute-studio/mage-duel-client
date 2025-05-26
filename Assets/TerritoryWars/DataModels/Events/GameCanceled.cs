using System;

namespace TerritoryWars.DataModels.Events
{
    [Serializable]
    public struct GameCanceled
    {
        public string HostPlayer;
        
        public GameCanceled SetData(evolute_duel_GameCanceled modelInstance)
        {
            HostPlayer = modelInstance.host_player.Hex();
            return this;
        }
    }
}