using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Managers.SessionComponents;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;

namespace TerritoryWars.Bots
{
    public class BotInputModule: BotModule
    {
        public override Bot Bot { get; set; }

        private TileSelector _tileSelector
        {
            get
            {
                if (SessionManager.Instance == null) return null;
                return SessionManager.Instance.TileSelector;
            }
        }
        
        public BotInputModule(Bot bot) : base(bot)
        {
        }

        public void PlaceTile(TileData tile, ValidPlacement placement, bool isJoker)
        {
            if (tile == null || placement == null)
            {
                CustomLogger.LogWarning("BotInputModule: Tile or placement is null");
                return;
            }
            
            //_tileSelector.PlaceTile(tile, placement, _localId);
            tile.Rotate(placement.rotation);
            var serverTypes = DojoConverter.MoveClientToServer(tile, placement.x, placement.y, isJoker);
            DojoConnector.MakeMove(Bot.Account, serverTypes.joker_tile, serverTypes.rotation, serverTypes.col, serverTypes.row);
        }
        
    }
}