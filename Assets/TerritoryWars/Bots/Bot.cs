using Dojo.Starknet;
using TerritoryWars.Dojo;
using TerritoryWars.General;

namespace TerritoryWars.Bots
{
    public class Bot
    {
        public GeneralAccount Account => AccountModule.Account;
        public bool IsDebug = false;
        
        public BotAccountModule AccountModule { get; private set; }
        public BotInputModule InputModule { get; private set; }
        public BotDataCollectorModule DataCollectorModule { get; private set; }
        public BotLogicModule LogicModule { get; private set; }
        public BotDebugModule DebugModule { get; private set; }
        //public BotEventsModule EventsModule { get; private set; }

        public void Initialize(GeneralAccount account)
        {
            AccountModule = new BotAccountModule(this, account);
            InputModule = new BotInputModule(this);
            DataCollectorModule = new BotDataCollectorModule(this);
            LogicModule = new BotLogicModule(this);
            DebugModule = new BotDebugModule(this);
            //EventsModule = new BotEventsModule(this);
        }

        public void SessionStarted()
        {
            //EventsModule.RegisterEvents(DojoGameManager.Instance.SessionManager);
        }

        public void MakeMove(){
            DataCollectorModule.CollectData();
            LogicModule.ExecuteLogic();
        }
        
        public void OnDestroy()
        {
            //EventsModule.UnregisterEvents();
        }
    }
}