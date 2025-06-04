using System;

namespace TerritoryWars.UI
{
    [Serializable]
    public struct ClientInput
    {
        public InputType Type;
        public ClientInput(InputType type)
        {
            Type = type;
        }
        public enum InputType
        {
            Move,
            Skip,
            RotateTile,
            UseJoker,
        }
    }
}
