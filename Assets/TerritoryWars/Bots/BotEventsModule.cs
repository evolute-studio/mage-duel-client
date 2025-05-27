using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.Bots
{
    public class BotEventsModule: BotModule
    {
        public override Bot Bot { get; set; }
        
        private DojoSessionManager _dojoSessionManager;
        
        public BotEventsModule(Bot bot) : base(bot)
        {
        }
        
        public void RegisterEvents(DojoSessionManager dojoSessionManager)
        {
            _dojoSessionManager = dojoSessionManager;
            _dojoSessionManager.OnMoveReceived += MoveReceivedHandler;
        }
        
        private void MoveReceivedHandler(string playerAddress, TileData tile, Vector2Int position, int rotation, bool isJoker)
        {
            if (playerAddress == Bot.Account.Address.Hex() || playerAddress != SessionManager.Instance.LocalPlayer.PlayerId)
            {
                return;
            }
            
            Bot.MakeMove();
        }
        
        public void UnregisterEvents()
        {
            _dojoSessionManager.OnMoveReceived -= MoveReceivedHandler;
        }
        
    }
}