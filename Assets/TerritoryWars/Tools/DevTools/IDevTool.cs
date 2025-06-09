namespace TerritoryWars.Tools.DevTools
{
    public interface IDevTool
    {
        string ToolName { get; }
        void DrawUI();
    }
}