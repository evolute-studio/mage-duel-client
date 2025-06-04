using System;

namespace TerritoryWars.DataModels
{
    [Serializable]
    public struct Rules
    {
        public string Id;
        public string[] Deck;
        public byte JokerNumber;

        public Rules SetData(evolute_duel_Rules rules)
        {
            return this;
        }
    }
}