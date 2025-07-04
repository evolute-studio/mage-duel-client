using System.Threading.Tasks;

namespace TerritoryWars.Managers.SessionComponents
{
    public abstract class LoopManager : ISessionComponent
    {
        public virtual void Initialize(SessionManagerContext managerContext) { }

        public void Dispose() { }

        public virtual void StartGame() { }

        public virtual async Task<byte> WhoseTurn() { return 1; }
    }
}