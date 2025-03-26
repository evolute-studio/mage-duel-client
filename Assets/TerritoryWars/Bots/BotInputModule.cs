namespace TerritoryWars.Bots
{
    public class BotInputModule: BotModule
    {
        public override Bot Bot { get; set; }
        
        public BotInputModule(Bot bot) : base(bot)
        {
        }
        
    }
}