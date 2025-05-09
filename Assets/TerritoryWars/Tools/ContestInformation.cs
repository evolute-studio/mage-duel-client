using System;

namespace TerritoryWars.Tools
{
    public class ContestInformation
    {
        public byte Root;
        public ContestType ContestType;
        public Action ContestAction;

        public ContestInformation(byte root, ContestType contestType, Action contestAction)
        {
            Root = root;
            ContestAction = contestAction;
            ContestType = contestType;
        }
    }

    public enum ContestType
    {
        Road,
        City,
        None
    }
}