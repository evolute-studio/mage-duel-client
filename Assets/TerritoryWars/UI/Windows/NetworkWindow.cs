using Dojo;
using TerritoryWars.DataModels.WebSocketEvents;
using TerritoryWars.General;

namespace TerritoryWars.UI.Windows
{
    public class NetworkWindow: Window
    {
        protected virtual void OnEventMessage(ModelInstance modelInstance) { }
        
        protected virtual void FetchData() { }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected virtual void OnPingEvent(PingEvent ping) { }
        
    }
}