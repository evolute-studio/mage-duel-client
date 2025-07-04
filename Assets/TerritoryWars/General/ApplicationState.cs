namespace TerritoryWars.General
{
    public enum ApplicationStates
    {
        Initializing,
        Menu,
        MatchTab,
        SnapshotTab,
        Session,
        Leaderboard,
        SpectatorTab,
        Spectating,
    }
    
    public static class ApplicationState
    {
        public static ApplicationStates CurrentState = ApplicationStates.Initializing;
        public static bool IsController = false;
        public static bool IsLoggedIn = false;
        
        public static void SetState(ApplicationStates state)
        {
            CurrentState = state;
        }
        
    }
}