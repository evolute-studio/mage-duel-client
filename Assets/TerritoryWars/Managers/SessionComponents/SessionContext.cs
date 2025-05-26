namespace TerritoryWars.Managers.SessionComponents
{
    public class SessionContext
    {
        public SessionManager SessionManager;
        public PlayersManager PlayersManager { get; set; }
        public GameLoopManager GameLoopManager { get; set; }
    }
}