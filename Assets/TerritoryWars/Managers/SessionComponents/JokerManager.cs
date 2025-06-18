using System.Collections.Generic;
using System.Linq;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class JokerManager: ISessionComponent
    {
        private SessionManagerContext _managerContext;
        private Board board => _managerContext.SessionContext.Board;
        private Player[] Players => _managerContext.SessionContext.Players;
        private Player _currentTurnPlayer => _managerContext.SessionContext.CurrentTurnPlayer;
        private BoardManager Board => _managerContext.BoardManager;
        private bool isJokerActive = false;
        public bool IsJokerActive => isJokerActive;
        private Dictionary<(int, int, int), string[]> _cachedCombinations = new Dictionary<(int, int, int), string[]>();
        private Dictionary<(int, int), int> _currentCombinationIndex = new Dictionary<(int, int), int>();

        public int UsedJokerCount = 3;
        public void Initialize(SessionManagerContext managerContext)
        {
            _managerContext = managerContext;
            GameUI.Instance.playerInfoUI.ShowPlayerJokerCount(_managerContext.SessionContext.LocalPlayer.PlayerSide);
            SetJokersCount(0, board.Player1.JokerCount);
            SetJokersCount(1, board.Player2.JokerCount);
        }
        
        public void ActivateJoker()
        {
            if (Players[_currentTurnPlayer.PlayerSide].JokerCount > 0)
            {
                isJokerActive = true;
                Players[_currentTurnPlayer.PlayerSide].JokerCount--;
                UsedJokerCount = Players[_currentTurnPlayer.PlayerSide].JokerCount--;
                _managerContext.TileSelector.StartJokerPlacement();
            }
        }
        
        public void DeactivateJoker()
        {
            isJokerActive = false;
            Players[_currentTurnPlayer.PlayerSide].JokerCount++;
            UsedJokerCount = Players[_currentTurnPlayer.PlayerSide].JokerCount++;
            GameUI.Instance.UpdateUI();
        }
        
        public void UpdateLocalPlayerJokerCount()
        {
            SetJokersCount(0, SessionManager.Instance.IsLocalPlayerHost ? UsedJokerCount : board.Player1.JokerCount);
            SetJokersCount(1, SessionManager.Instance.IsLocalPlayerHost ? board.Player2.JokerCount : UsedJokerCount);
        }
        
        public TileData GetGenerateJokerTile(int x, int y)
        {
            int moveNumber = board.MovesCount;
            if (!_cachedCombinations.ContainsKey((x, y, moveNumber)))
            {
                _cachedCombinations[(x, y, moveNumber)] = GenerateAllCombinations(x, y);
                _currentCombinationIndex[(x, y)] = 0;
            }
            
            string[] possibleCombinations = _cachedCombinations[(x, y, moveNumber)];
            
            if (!_currentCombinationIndex.ContainsKey((x, y)))
            {
                _currentCombinationIndex[(x, y)] = 0;
            }
            
            int currentIndex = _currentCombinationIndex[(x, y)];
            string tileConfig = possibleCombinations[currentIndex];
            
            _currentCombinationIndex[(x, y)] = (currentIndex + 1) % possibleCombinations.Length;
            
            // TODO: Mb here will be problem with player side, need to check
            TileData jokerTile = new TileData(tileConfig, Vector2Int.zero, _managerContext.SessionContext.CurrentTurnPlayer.PlayerSide);
            return jokerTile;
        }
        
        public static TileData GetOneJokerCombination(int x, int y)
        {
            JokerManager jokerManager = new JokerManager();
            jokerManager.Initialize(SessionManager.Instance.ManagerContext);
            string[] possibleCombinations = jokerManager.GenerateAllCombinations(x, y);
            string tileConfig = possibleCombinations[Random.Range(0, possibleCombinations.Length)];
            TileData jokerTile = new TileData(tileConfig, new Vector2Int(x, y), SessionManager.Instance.ManagerContext.SessionContext.CurrentTurnPlayer.PlayerSide);
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
            int characterId = _currentTurnPlayer == null ? 0 : _currentTurnPlayer.PlayerSide;
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
            GameUI.Instance.playerInfoUI.ShowPlayerJokerCount(_managerContext.SessionContext.LocalPlayer.PlayerSide);
        }

        public void Dispose()
        {
            
        }
        
        
        
        
        
        
        
        
         
        
        
        
        

        

        
        
        

        
    }
}