using Dojo.Starknet;

namespace TerritoryWars.Bots
{
    public class Bot
    {
        public Account Account => AccountModule.Account;
        public BotAccountModule AccountModule { get; private set; }
        public BotInputModule InputModule { get; private set; }
        public BotLogicModule LogicModule { get; private set; }

        public void Initialize(Account account)
        {
            AccountModule = new BotAccountModule(this, account);
            InputModule = new BotInputModule(this);
            LogicModule = new BotLogicModule(this);
        }
    }
}