using System.Collections.Generic;
using Dojo.Starknet;
using NUnit.Framework;
using TerritoryWars.General;
using UnityEngine;

namespace TerritoryWars.Bots
{
    public class BotAccountModule: BotModule
    {
        public override Bot Bot { get; set; }
        public GeneralAccount Account { get; }
        
        
        public BotAccountModule(Bot bot, GeneralAccount account) : base(bot)
        {
            Account = account;
        }

        public string GetDefaultUsername()
        {
            return "Bot" + Random.Range(1, 1000);
        }
    }
}