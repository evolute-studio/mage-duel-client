namespace TerritoryWars.Managers.SessionComponents
{
    public class SessionManagerContext
    {
        public SessionContext SessionContext { get; set; }
        public SessionManager SessionManager;
        public PlayersManager PlayersManager { get; set; }
        public GameLoopManager GameLoopManager { get; set; }
    }
}