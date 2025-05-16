using Dojo;

namespace TerritoryWars.UI.Windows
{
    public abstract class NetworkWindow: Window
    {
        protected virtual void OnEventMessage(ModelInstance modelInstance) { }
        
        protected virtual void FetchData() { }
    }
}