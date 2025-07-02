using System.Collections;
using System.Collections.Generic;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.Bots
{
    public class BotDebugModule: BotModule
    {
        public override Bot Bot { get; set; }
        
        public bool IsAutoMove = true;
        public bool IsJoker = false;
        private float _autoMoveDelay = 1f;
        private Dictionary<ValidPlacement, float> _allMoves;
        private TileData _tileData;
        private ValidPlacement _validPlacement;
        
        public BotDebugModule(Bot bot) : base(bot)
        {
            
        }

        public void ShowMoves(Dictionary<ValidPlacement, float> allMoves)
        {
            _allMoves = allMoves;
        }
        
        public void SetMoveVariant(TileData tileData, ValidPlacement validPlacement)
        {
            _tileData = tileData;
            _validPlacement = validPlacement;
            
            if (IsAutoMove)
            {
                Coroutines.StartRoutine(AutoMoveCoroutine());
            }
            
        }
        
        private IEnumerator AutoMoveCoroutine()
        {
            yield return new WaitForSeconds(_autoMoveDelay);
            Bot.LogicModule.MakeMove();
        }

        public void MakeMove()
        {
            if (IsJoker) MakeJokerMove();
            else MakeSimpleMove();
        }

        public void Recalculate()
        {
            if (IsJoker) CalculateJokerMove();
            else CalculateMove();
        }
        
        public void CalculateMove()
        {
            Bot.DataCollectorModule.CollectData();
            var allMoves = Bot.LogicModule.EvaluateAllMoves(Bot.DataCollectorModule.CurrentTile, Bot.DataCollectorModule.CurrentValidPlacements);
            ShowMoves(allMoves);
            SetMoveVariant(Bot.DataCollectorModule.CurrentTile, Bot.LogicModule.FindBestMove(Bot.DataCollectorModule.CurrentTile, Bot.DataCollectorModule.CurrentValidPlacements));

        }
        
        public void CalculateJokerMove()
        {
            Bot.DataCollectorModule.CollectJokerData();
            _allMoves = Bot.LogicModule.EvaluateAllJokerMoves(Bot.DataCollectorModule.CurrentJokers);
        }
        
        public void MakeSimpleMove()
        {
            if (_tileData == null || _validPlacement == null) Bot.LogicModule.SkipMove();
            Bot.LogicModule.PlaceTile(_tileData, _validPlacement, false);
            _allMoves = null;
        }

        public void MakeJokerMove()
        {
            if (_allMoves == null) return;
            var bestJokerMove = Bot.LogicModule.GetJokerMoveVariant();
            Bot.LogicModule.PlaceTile(bestJokerMove.Item1, bestJokerMove.Item2, true);
            _allMoves = null;
        }

        public void OnGUI()
        {
            // auto move toggle
            // startPos os center of left side
            Vector2 startPos = new Vector2(10, Screen.height / 2f);
            IsAutoMove = GUI.Toggle(new Rect(startPos.x, startPos.y, 100, 50), IsAutoMove, "Auto Move");
            IsJoker = GUI.Toggle(new Rect(startPos.x, startPos.y + 50, 100, 50), IsJoker, "Is Joker");
            GUI.Label(new Rect(startPos.x, startPos.y + 100, 100, 50), $"JokerChance: {Bot.LogicModule.GetJokerChance()}");

            
            
            if (!Bot.IsDebug || _allMoves == null) return;
            foreach (var move in _allMoves)
            {
                BoardManager board = Bot.DataCollectorModule.Board;
                var placement = move.Key;
                Vector3 worldTilePosition = BoardManager.GetTilePosition(placement.x, placement.y);
                Vector3 screenTilePosition = Camera.main.WorldToScreenPoint(worldTilePosition);
                
                GUI.Label(new Rect(screenTilePosition.x, Screen.height - screenTilePosition.y, 100, 100), move.Value.ToString());
                
            }
            if (GUI.Button(new Rect(startPos.x, startPos.y + 150, 100, 50), "Recalculate"))
            {
                Recalculate();
            }  
            // button make move
            if (GUI.Button(new Rect(startPos.x, startPos.y + 200, 100, 50), "Make Move"))
            {
                MakeMove();
            }    
            
        }
    }
}