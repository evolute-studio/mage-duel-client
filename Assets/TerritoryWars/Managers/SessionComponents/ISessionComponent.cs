namespace TerritoryWars.Managers.SessionComponents
{
    public interface ISessionComponent
    {
        void Initialize(SessionContext context);
        void Dispose();
    }
}