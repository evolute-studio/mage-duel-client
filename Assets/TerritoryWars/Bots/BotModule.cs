namespace TerritoryWars.Bots
{
    public abstract class BotModule
    {
        public abstract Bot Bot { get; set; }

        public BotModule(Bot bot)
        {
            Bot = bot;
        }
    }
}