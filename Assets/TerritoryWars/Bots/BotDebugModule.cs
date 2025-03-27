using System.Collections;
using System.Collections.Generic;
using TerritoryWars.General;
using TerritoryWars.Tile;
using UnityEngine;

namespace TerritoryWars.Bots
{
    public class BotDebugModule: BotModule
    {
        public override Bot Bot { get; set; }
        
        public bool IsAutoMove = true;
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
            MakeMove();
        }

        public void MakeMove()
        {
            if (_tileData == null || _validPlacement == null) Bot.LogicModule.SkipMove();
            Bot.LogicModule.PlaceTile(_tileData, _validPlacement);
            _allMoves = null;
        }

        public void OnGUI()
        {
            // auto move toggle
            IsAutoMove = GUI.Toggle(new Rect(10, 70, 100, 50), IsAutoMove, "Auto Move");
            if (!Bot.IsDebug || _allMoves == null) return;
            foreach (var move in _allMoves)
            {
                Board board = Bot.DataCollectorModule.Board;
                var placement = move.Key;
                Vector3 worldTilePosition = board.GetTilePosition(placement.X, placement.Y);
                Vector3 screenTilePosition = Camera.main.WorldToScreenPoint(worldTilePosition);
                
                GUI.Label(new Rect(screenTilePosition.x, Screen.height - screenTilePosition.y, 100, 100), move.Value.ToString());
                
            }
            // button make move
            if (GUI.Button(new Rect(10, 10, 100, 50), "Make Move"))
            {
                MakeMove();
            }    
            
        }
    }
}