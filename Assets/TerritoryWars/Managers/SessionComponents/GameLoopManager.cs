namespace TerritoryWars.Managers.SessionComponents
{
    public class GameLoopManager: ISessionComponent
    {
        private SessionContext _context;

        public void Initialize(SessionContext context)
        {
            _context = context;
        }

        public void StartGame() { }
        public void EndGame() { }

        private void StartTurn() { }

        public void EndTurn() { }

        public void Dispose() { }
    }
}