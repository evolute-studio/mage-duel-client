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
            EventBus.Subscribe<PingEvent>(OnPingEvent);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            EventBus.Unsubscribe<PingEvent>(OnPingEvent);
        }

        protected virtual void OnPingEvent(PingEvent ping) { }
        
    }
}