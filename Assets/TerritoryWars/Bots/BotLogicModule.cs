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
                return _player.LocalId;
            }
        }
        
        private Character _player 
        {
            get
            {
                if (SessionManager.Instance == null) return null;
                return SessionManager.Instance.GetPlayerByAddress(Bot.Account.Address.Hex());
            }
        }

        public BotLogicModule(Bot bot) : base(bot)
        {
        }

        public void ExecuteLogic()
        {
            if (Bot.IsDebug)
            {
                Bot.DebugModule.Recalculate();
                return;
            }

            MakeMove();
        }

        public bool IsSimpleMove()
        {
            bool isJoker = Random.value < GetJokerChance();
            bool hasJokers = _player.JokerCount > 0;
            return !isJoker || !hasJokers;
        }
        
        public float GetJokerChance()
        {
            float k = 4f;
            const int maxTiles = 64;
            const int borderTiles = 36;
            const int maxJokers = 6;
            int maxMoves = maxTiles + maxJokers;
            int placedTiles = Bot.DataCollectorModule.Board.PlacedTiles.Count;

            if (placedTiles <= borderTiles) return 0f;

            float progress = (float)(placedTiles - borderTiles) / maxMoves;
            float chance = Mathf.Clamp01(Mathf.Pow(progress, k));
            return chance;
        }

        public void MakeMove()
        {
            if (IsSimpleMove())
            {
                MakeSimpleMove();
            }
            else
            {
                MakeJokerMove();
            }
        }
        
        public void MakeSimpleMove()
        {
            (TileData tileData, ValidPlacement validPlacement) = SelectMoveVariant();
            if (tileData == null || validPlacement == null)
            {
                CustomLogger.LogDojoLoop("BotLogicModule: Skip move");
                if (_player.JokerCount > 0)
                {
                    MakeJokerMove();
                    return;
                }
                SkipMove();
                return;
            }
            PlaceTile(tileData, validPlacement, false);
        }
        
        public void MakeJokerMove()
        {
            (TileData tileData, ValidPlacement validPlacement) = GetJokerMoveVariant();
            if (tileData == null || validPlacement == null)
            {
                CustomLogger.LogDojoLoop("BotLogicModule: Skip joker move");
                SkipMove();
                return;
            }
            PlaceTile(tileData, validPlacement, true);
        }

        public void PlaceTile(TileData tileData, ValidPlacement validPlacement, bool isJoker)
        {
            Bot.InputModule.PlaceTile(tileData, validPlacement, isJoker);
        }
        
        public void SkipMove()
        {
            DojoConnector.SkipMove(Bot.Account);
        }
        
        

        public (TileData, ValidPlacement) GetJokerMoveVariant()
        {
            Bot.DataCollectorModule.CollectJokerData();
            Dictionary<ValidPlacement, TileData> jokers = Bot.DataCollectorModule.CurrentJokers;
            if (jokers == null || jokers.Count == 0)
            {
                CustomLogger.LogWarning("BotLogicModule: No jokers found");
                return (null, null);
            }
            // evaluate every jokers
            Dictionary<ValidPlacement, float> jokerValues = EvaluateAllJokerMoves(jokers);
            var bestJoker = jokerValues.OrderByDescending(x => x.Value).First();
            ValidPlacement key = null;
            foreach (var joker in jokers)
            {
                if (joker.Key.GetHashCode() == bestJoker.Key.GetHashCode())
                {
                    key = joker.Key;
                    break;
                }
            }
            return (jokers[key], bestJoker.Key);
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

        public ValidPlacement FindBestMove(TileData tileData, List<ValidPlacement> validPlacement)
        {
            Dictionary<ValidPlacement, float> moves = EvaluateAllMoves(tileData, validPlacement);
            if (moves.Count == 0)
            {
                CustomLogger.LogWarning("BotLogicModule: No moves found");
                return null;
            }
            var result = moves.OrderByDescending(x => x.Value).First();
            return result.Key;
        }
        
        public Dictionary<ValidPlacement, float> EvaluateAllMoves(TileData tileData, List<ValidPlacement> validPlacements)
        {
            Dictionary<ValidPlacement, float> moves = new Dictionary<ValidPlacement, float>();
            foreach (var placement in validPlacements)
            {
                moves.Add(placement, EvaluateMove(tileData, placement));
            }
            return moves;
        }
        
        public Dictionary<ValidPlacement, float> EvaluateAllJokerMoves(Dictionary<ValidPlacement, TileData> jokers)
        {
            Dictionary<ValidPlacement, float> moves = new Dictionary<ValidPlacement, float>();
            foreach (var joker in jokers)
            {
                moves.Add(joker.Key, EvaluateMove(joker.Value, joker.Key));
            }
            return moves;
        }

        private float EvaluateMove(TileData tileData, ValidPlacement validPlacement)
        {
            TileData tile = new TileData(tileData.id);
            tile.Rotate(validPlacement.rotation);

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
                            new Vector2Int(validPlacement.x, validPlacement.y), (Side)i);
                    if (citySet.Key == null || processedCities.Contains(citySet.Key)) continue;
                    cityValue += EvaluateStructure(citySet);
                    processedCities.Add(citySet.Key);
                }
                if (sideType == 'R')
                {
                    var roadSet =
                        _sessionManager.GetNearSetByPositionAndSide<evolute_duel_RoadNode>(
                            new Vector2Int(validPlacement.x, validPlacement.y), (Side)i);
                    if (roadSet.Key == null || processedRoads.Contains(roadSet.Key)) continue;
                    roadValue += EvaluateStructure(roadSet);
                    processedRoads.Add(roadSet.Key);
                }
            }
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
            return result;
        }
    }
}