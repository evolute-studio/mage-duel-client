namespace TerritoryWars.DataModels.ClientEvents
{
    public struct TimerEvent
    {
        public TimerEventType Type { get; }
        public TimerProgressType ProgressType { get; set; }
        public ulong StartTimestamp { get; set; }

        public TimerEvent(TimerEventType type, TimerProgressType progressType, ulong startTimestamp = 0)
        {
            StartTimestamp = startTimestamp;
            Type = type;
            ProgressType = progressType;
        }
    }

    public enum TimerEventType
    {
        //Started,
        //TurnTimeElapsed,
        //PassingTimeElapsed,
        
        // phases
        GameCreation,
        Revealing,
        Requesting,
        Moving,
        // client
        Passing,
    }

    public enum TimerProgressType
    {
        Started,
        Elapsed,
        ElapsedCompletely,
    }
}
