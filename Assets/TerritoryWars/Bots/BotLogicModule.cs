using System.Collections.Generic;
using System.Linq;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;
using Microsoft.CSharp;
using TerritoryWars.Models;

namespace TerritoryWars.Bots
{
    public class BotLogicModule : BotModule
    {
        public override Bot Bot { get; set; }

        private DojoSessionManager _sessionManager
        {
            get
            {
                if (SessionManager.Instance == null) return null;
                return DojoGameManager.Instance.SessionManager;
            }
        }

        private int _localId
        {
            get
            {
                if (SessionManager.Instance == null) return -1;
                return SessionManager.Instance.RemotePlayer.LocalId;
            }
        }

        public BotLogicModule(Bot bot) : base(bot)
        {
        }

        public void ExecuteLogic()
        {
            if (Bot.IsDebug)
            {
                var allMoves = EvaluateAllMoves(Bot.DataCollectorModule.CurrentTile, Bot.DataCollectorModule.CurrentValidPlacements);
                Bot.DebugModule.ShowMoves(allMoves);
                Bot.DebugModule.SetMoveVariant(Bot.DataCollectorModule.CurrentTile, FindBestMove(Bot.DataCollectorModule.CurrentTile, Bot.DataCollectorModule.CurrentValidPlacements));
                return;
            }
            MakeMove();
        }

        public void PlaceTile(TileData tileData, ValidPlacement validPlacement)
        {
            Bot.InputModule.PlaceTile(tileData, validPlacement, false);
        }
        public void MakeMove()
        {
            (TileData tileData, ValidPlacement validPlacement) = SelectMoveVariant();
            if (tileData == null || validPlacement == null)
            {
                CustomLogger.LogDojoLoop("BotLogicModule: Skip move");
                SkipMove();
                return;
            }
            PlaceTile(tileData, validPlacement);
        }
        
        public void SkipMove()
        {
            DojoConnector.SkipMove(Bot.Account);
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

            ValidPlacement selectedPlacement = FindBestMove(tileData, validPlacements);
            return (tileData, selectedPlacement);
        }

        private ValidPlacement FindBestMove(TileData tileData, List<ValidPlacement> validPlacement)
        {
            Dictionary<ValidPlacement, float> moves = EvaluateAllMoves(tileData, validPlacement);
            if (moves.Count == 0)
            {
                CustomLogger.LogWarning("BotLogicModule: No moves found");
                return null;
            }
            var result = moves.OrderByDescending(x => x.Value).First();
            CustomLogger.LogImportant($"Best move: {result.Key.X}, {result.Key.Y}, {result.Key.Rotation}, Value: {result.Value}");
            return result.Key;
        }
        
        private Dictionary<ValidPlacement, float> EvaluateAllMoves(TileData tileData, List<ValidPlacement> validPlacements)
        {
            Dictionary<ValidPlacement, float> moves = new Dictionary<ValidPlacement, float>();
            foreach (var placement in validPlacements)
            {
                moves.Add(placement, EvaluateMove(tileData, placement));
            }
            return moves;
        }

        private float EvaluateMove(TileData tileData, ValidPlacement validPlacement)
        {
            CustomLogger.LogImportant($"Evaluating move. Config: {tileData.id}, X: {validPlacement.X}, Y: {validPlacement.Y}, Rotation: {validPlacement.Rotation}");
            TileData tile = new TileData(tileData.id);
            tile.Rotate(validPlacement.Rotation);

            (float basicCityValue, float basicRoad) = EvaluateBasicValue(tile);
            float cityValue = basicCityValue;
            float roadValue = basicRoad;
            
            List<evolute_duel_CityNode> processedCities = new List<evolute_duel_CityNode>();
            List<evolute_duel_RoadNode> processedRoads = new List<evolute_duel_RoadNode>();
            
            for (int i = 0; i < 4; i++)
            {
                char sideType = tile.id[i];
                if (sideType == 'C')
                {
                    var citySet =
                        _sessionManager.GetNearSetByPositionAndSide<evolute_duel_CityNode>(
                            new Vector2Int(validPlacement.X, validPlacement.Y), (Side)i);
                    if (citySet.Key == null || processedCities.Contains(citySet.Key)) continue;
                    cityValue += EvaluateStructure(citySet);
                    processedCities.Add(citySet.Key);
                }
                if (sideType == 'R')
                {
                    var roadSet =
                        _sessionManager.GetNearSetByPositionAndSide<evolute_duel_RoadNode>(
                            new Vector2Int(validPlacement.X, validPlacement.Y), (Side)i);
                    if (roadSet.Key == null || processedRoads.Contains(roadSet.Key)) continue;
                    roadValue += EvaluateStructure(roadSet);
                    processedRoads.Add(roadSet.Key);
                }
            }
            CustomLogger.LogImportant($"Evaluating move. City value: {cityValue}, Road value: {roadValue}, Total value: {cityValue + roadValue}");
            return cityValue + roadValue;
        }
        
        private (float cityValue, float roadValue) EvaluateBasicValue(TileData tileData)
        {
            float CITY_WEIGHT = 2f;
            float ROAD_WEIGHT = 1f;
            
            int cityCount = tileData.id.Count(c => c == 'C');
            int roadCount = tileData.id.Count(c => c == 'R');
            
            return (cityCount * CITY_WEIGHT, + roadCount * ROAD_WEIGHT);
        }

        private float EvaluateStructure<T>(KeyValuePair<T, List<T>> kvp) where T : class
        {
            const float OPEN_EDGE_WEIGHT = 0.5f;
            const float POINTS_WEIGHT = 1f;

            INode structure = kvp.Key as INode;
            if (structure == null) return 0;
            
            int bluePoints = structure.GetBluePoints();
            int redPoints = structure.GetRedPoints();
            int openEdges = structure.GetOpenEdges();

            int myPoints = _localId == 0 ? bluePoints : redPoints;
            int enemyPoints = _localId == 0 ? redPoints : bluePoints;

            int deltaPoints = myPoints - enemyPoints;
            float result = deltaPoints * POINTS_WEIGHT;
            CustomLogger.LogImportant($"Evaluating structure. Points: {deltaPoints}, Result: {result}");
            return result;
        }
    }
}