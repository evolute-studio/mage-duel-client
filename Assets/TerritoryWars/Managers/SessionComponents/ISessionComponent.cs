namespace TerritoryWars.Managers.SessionComponents
{
    public interface ISessionComponent
    {
        void Initialize(SessionManagerContext managerContext);
        void Dispose();
    }
}