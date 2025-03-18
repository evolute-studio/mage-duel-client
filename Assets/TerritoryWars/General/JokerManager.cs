using System;
using System.Collections.Generic;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.UI;
using Random = UnityEngine.Random;

namespace TerritoryWars.General
{
    public class JokerManager
    {
        private SessionManager _sessionManager;
        private Character[] Players => _sessionManager.Players;
        private Character CurrentTurnPlayer => _sessionManager.CurrentTurnPlayer;
        private Board Board => _sessionManager.Board;
        
        private bool isJokerActive = false;
        
        public bool IsJokerActive => isJokerActive;

        public JokerManager(SessionManager manager)
        {
            _sessionManager = manager;
        }
        public void Initialize(evolute_duel_Board board)
        {
            GameUI.Instance.SessionUI.ShowPlayerJokerCount(_sessionManager.LocalPlayer.LocalId);
            SetJokersCount(0, board.player1.Item3);
            SetJokersCount(1, board.player2.Item3);
        }
        
         public void ActivateJoker()
        {
            if (Players[CurrentTurnPlayer.LocalId].JokerCount > 0)
            {
                isJokerActive = true;
                Players[CurrentTurnPlayer.LocalId].JokerCount--;
                _sessionManager.TileSelector.StartJokerPlacement();
            }
        }
        
        public void DeactivateJoker()
        {
            isJokerActive = false;
            Players[CurrentTurnPlayer.LocalId].JokerCount++;
            GameUI.Instance.UpdateUI();
        }
        
        public TileData GetGenerateJokerTile(int x, int y)
        {
            
            Dictionary<Side, LandscapeType> neighborSides = new Dictionary<Side, LandscapeType>();
            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + Board.GetXOffset(side);
                int newY = y + Board.GetYOffset(side);
                
                if (Board.IsValidPosition(newX, newY) && Board.GetTileData(newX, newY) != null)
                {
                    var neighborTile = Board.GetTileData(newX, newY);
                    neighborSides[side] = neighborTile.GetSide(Board.GetOppositeSide(side));
                }
            }
            
            char[] baseSides = new char[4];
            char[] randomSides = new char[4];
            for (int i = 0; i < 4; i++)
            {
                Side side = (Side)i;
                if (neighborSides.ContainsKey(side))
                {
                    baseSides[i] = LandscapeToChar(neighborSides[side]);
                    randomSides[i] = LandscapeToChar(neighborSides[side]);
                }
                else
                {
                    baseSides[i] = 'X';
                    randomSides[i] = GetRandomLandscape();
                }
            }
            
            string baseTileConfig = new string(baseSides);
            string randomTileConfig = new string(randomSides);
            
            (string tileConfig, int rotation) = OnChainBoardDataConverter.GetRandomTypeAndRotationFromDeck(baseTileConfig);
            string configResult = String.IsNullOrEmpty(tileConfig) ? randomTileConfig : tileConfig;
            TileData jokerTile = new TileData(randomTileConfig);
            //jokerTile.Rotate((rotation + 2) % 4);
            return jokerTile;
        }
        
        private char GetRandomLandscape()
        {
            float random = Random.value;
            if (random < 0.4f) return 'F';      
            else if (random < 0.7f) return 'R';  
            else return 'C';                     
        }
        
        private char LandscapeToChar(LandscapeType type)
        {
            return type switch
            {
                LandscapeType.City => 'C',
                LandscapeType.Road => 'R',
                LandscapeType.Field => 'F',
                _ => 'F'
            };
        }
        
        public bool CanUseJoker()
        {
            int characterId = CurrentTurnPlayer == null ? 0 : CurrentTurnPlayer.LocalId;
            return !isJokerActive && Players[characterId].JokerCount > 0;
        }

        public void CompleteJokerPlacement()
        {
            isJokerActive = false;
            GameUI.Instance.SetJokerMode(false);
            GameUI.Instance.UpdateUI();
        }
        
        public void SetJokersCount(int playerId, int count)
        {
            GameUI.Instance.SessionUI.SetJokersCount(playerId, count);
            GameUI.Instance.SessionUI.ShowPlayerJokerCount(_sessionManager.LocalPlayer.LocalId);
        }
    }
}