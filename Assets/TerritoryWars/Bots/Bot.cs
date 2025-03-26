using Dojo.Starknet;
using TerritoryWars.Dojo;

namespace TerritoryWars.Bots
{
    public class Bot
    {
        public Account Account => AccountModule.Account;
        public BotAccountModule AccountModule { get; private set; }
        public BotInputModule InputModule { get; private set; }
        public BotDataCollectorModule DataCollectorModule { get; private set; }
        public BotLogicModule LogicModule { get; private set; }
        //public BotEventsModule EventsModule { get; private set; }

        public void Initialize(Account account)
        {
            AccountModule = new BotAccountModule(this, account);
            InputModule = new BotInputModule(this);
            DataCollectorModule = new BotDataCollectorModule(this);
            LogicModule = new BotLogicModule(this);
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