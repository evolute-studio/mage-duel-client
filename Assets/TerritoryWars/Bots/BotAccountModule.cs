using System.Collections.Generic;
using Dojo.Starknet;
using NUnit.Framework;
using UnityEngine;

namespace TerritoryWars.Bots
{
    public class BotAccountModule: BotModule
    {
        public override Bot Bot { get; set; }
        public Account Account { get; }
        
        
        public BotAccountModule(Bot bot, Account account) : base(bot)
        {
            Account = account;
        }

        public string GetDefaultUsername()
        {
            return "Bot" + Random.Range(1, 1000);
        }
    }
}