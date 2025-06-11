namespace TerritoryWars.DataModels.ClientEvents
{
    public struct TimerEvent
    {
        public TimerEventType Type { get; }
        public ulong StartTimestamp { get; set; }

        public TimerEvent(TimerEventType type, ulong startTimestamp = 0)
        {
            StartTimestamp = startTimestamp;
            Type = type;
        }
    }

    public enum TimerEventType
    {
        Started,
        TurnTimeElapsed,
        PassingTimeElapsed,
        
        GameCreation,
        Revealing,
        Requesting,
        Moving,
    }
}
