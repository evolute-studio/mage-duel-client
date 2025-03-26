using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
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

        private int _localId
        {
            get
            {
                if (SessionManager.Instance == null) return -1;
                return SessionManager.Instance.RemotePlayer.LocalId;
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
            tile.Rotate(placement.Rotation);
            var serverTypes = DojoConverter.MoveClientToServer(tile, placement.X, placement.Y, isJoker);
            DojoConnector.MakeMove(Bot.Account, serverTypes.joker_tile, serverTypes.rotation, serverTypes.col, serverTypes.row);
        }
        
    }
}