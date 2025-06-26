using TerritoryWars.DataModels;
using TerritoryWars.General;

namespace TerritoryWars.Managers.SessionComponents
{
    public class SessionManagerContext
    {
        public SessionContext SessionContext { get; set; }
        public SessionManager SessionManager;
        public PlayersManager PlayersManager { get; set; }
        public GameLoopManager GameLoopManager { get; set; }
        
        public JokerManager JokerManager { get; set; }
        public ContestManager ContestManager { get; set; }
        
        public BoardManager BoardManager => SessionManager.BoardManager;
        public TileSelector TileSelector => SessionManager.TileSelector;
        public StructureHoverManager StructureHoverManager => SessionManager.StructureHoverManager;
    }
}