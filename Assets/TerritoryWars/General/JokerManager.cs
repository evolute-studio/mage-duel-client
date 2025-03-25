using System;
using System.Collections.Generic;
using System.Linq;
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
        
        private Dictionary<(int, int), string[]> _cachedCombinations = new Dictionary<(int, int), string[]>();
        private Dictionary<(int, int), int> _currentCombinationIndex = new Dictionary<(int, int), int>();
        
        public bool IsJokerActive => isJokerActive;

        public JokerManager(SessionManager manager)
        {
            _sessionManager = manager;
        }
        public void Initialize(evolute_duel_Board board)
        {
            GameUI.Instance.playerInfoUI.ShowPlayerJokerCount(_sessionManager.LocalPlayer.LocalId);
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
            if (!_cachedCombinations.ContainsKey((x, y)))
            {
                _cachedCombinations[(x, y)] = GenerateAllCombinations(x, y);
                _currentCombinationIndex[(x, y)] = 0;
            }
            
            string[] possibleCombinations = _cachedCombinations[(x, y)];
            
            if (!_currentCombinationIndex.ContainsKey((x, y)))
            {
                _currentCombinationIndex[(x, y)] = 0;
            }
            
            int currentIndex = _currentCombinationIndex[(x, y)];
            string tileConfig = possibleCombinations[currentIndex];
            
            _currentCombinationIndex[(x, y)] = (currentIndex + 1) % possibleCombinations.Length;
            
            TileData jokerTile = new TileData(tileConfig);
            return jokerTile;
        }

        public string[] GenerateAllCombinations(int x, int y)
        {
            Dictionary<Side, LandscapeType> neighborSides = GetNeighborSides(x, y);
            
            char[] template = new char[4];
            bool[] fixedSides = new bool[4];
            bool[] universalSides = new bool[4];
            
            for (int i = 0; i < 4; i++)
            {
                Side side = (Side)i;
                if (neighborSides.ContainsKey(side))
                {
                    if (neighborSides[side] == LandscapeType.Universal)
                    {
                        template[i] = 'X';
                        fixedSides[i] = false;
                        universalSides[i] = true;
                    }
                    else
                    {
                        template[i] = LandscapeToChar(neighborSides[side]);
                        fixedSides[i] = true;
                        universalSides[i] = false;
                    }
                }
                else
                {
                    template[i] = 'X';
                    fixedSides[i] = false;
                    universalSides[i] = false;
                }
            }
            
            HashSet<string> uniqueCombinations = new HashSet<string>();
            GenerateCombinationsRecursive(template, fixedSides, universalSides, 0, uniqueCombinations);
            
            return uniqueCombinations.ToArray();
        }
        
        private void GenerateCombinationsRecursive(char[] template, bool[] fixedSides, bool[] universalSides, int index, HashSet<string> combinations)
        {
            if (index >= 4)
            {
                combinations.Add(new string(template));
                return;
            }
            
            if (fixedSides[index])
            {
                GenerateCombinationsRecursive(template, fixedSides, universalSides, index + 1, combinations);
            }
            else
            {
                char[] possibleTypes = { 'F', 'R', 'C' };
                foreach (char type in possibleTypes)
                {
                    template[index] = type;
                    GenerateCombinationsRecursive(template, fixedSides, universalSides, index + 1, combinations);
                }
            }
        }

        private Dictionary<Side, LandscapeType> GetNeighborSides(int x, int y)
        {
            Dictionary<Side, LandscapeType> neighborSides = new Dictionary<Side, LandscapeType>();
            foreach (Side side in System.Enum.GetValues(typeof(Side)))
            {
                int newX = x + Board.GetXOffset(side);
                int newY = y + Board.GetYOffset(side);
                
                if (Board.IsValidPosition(newX, newY) && Board.GetTileData(newX, newY) != null)
                {
                    var neighborTile = Board.GetTileData(newX, newY);
                    bool isEdgeTile = Board.IsEdgeTile(newX, newY);
                    
                    if (isEdgeTile && neighborTile.GetSide(Board.GetOppositeSide(side)) == LandscapeType.Field)
                    {
                        neighborSides[side] = LandscapeType.Universal;
                    }
                    else
                    {
                        neighborSides[side] = neighborTile.GetSide(Board.GetOppositeSide(side));
                    }
                    
                }
            }
            return neighborSides;
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
            GameUI.Instance.playerInfoUI.SetJokersCount(playerId, count);
            GameUI.Instance.playerInfoUI.ShowPlayerJokerCount(_sessionManager.LocalPlayer.LocalId);
        }
    }
}