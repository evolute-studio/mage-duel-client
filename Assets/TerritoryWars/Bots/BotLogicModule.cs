namespace TerritoryWars.Bots
{
    public class BotLogicModule: BotModule
    {
        public override Bot Bot { get; set; }
        
        public BotLogicModule(Bot bot) : base(bot)
        {
        }
        
    }
}