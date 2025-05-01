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
    }
    
    public static class ApplicationState
    {
        public static ApplicationStates CurrentState = ApplicationStates.Initializing;
        public static bool IsController = false;
        
        public static void SetState(ApplicationStates state)
        {
            CurrentState = state;
        }
        
    }
}