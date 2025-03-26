using System.Collections.Generic;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.Bots
{
    public class BotLogicModule: BotModule
    {
        public override Bot Bot { get; set; }
        
        public BotLogicModule(Bot bot) : base(bot)
        {
        }
        
        public void ExecuteLogic()
        {
            MakeMove();
        }

        public void MakeMove()
        {
            (TileData tileData, ValidPlacement validPlacement) = SelectMoveVariant();
            if (tileData == null || validPlacement == null)
            {
                CustomLogger.LogDojoLoop("BotLogicModule: Skip move");
                DojoConnector.SkipMove(Bot.Account);
                return;
            }
            
            Bot.InputModule.PlaceTile(tileData, validPlacement, false);
        }

        private (TileData, ValidPlacement) SelectMoveVariant()
        {
            TileData tileData = Bot.DataCollectorModule.CurrentTile;
            List<ValidPlacement> validPlacements = Bot.DataCollectorModule.CurrentValidPlacements;
            
            if (tileData == null || validPlacements == null || validPlacements.Count == 0)
            {
                CustomLogger.LogWarning("BotLogicModule: Tile or placements are null or empty");
                return (null, null);
            }
            
            ValidPlacement randomPlacement = validPlacements[Random.Range(0, validPlacements.Count)];
            return (tileData, randomPlacement);
        }
        
    }
}