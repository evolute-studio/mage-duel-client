using System.Collections.Generic;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;
using Vector2Int = UnityEngine.Vector2Int;

namespace TerritoryWars.Bots
{
    public class BotDataCollectorModule: BotModule
    {
        public override Bot Bot { get; set; }
        
        private BoardManager _board;

        public BoardManager Board
        {
            get
            {
                if (SessionManagerOld.Instance == null)
                {
                    CustomLogger.LogWarning("BotDataCollectorModule: SessionManager is null");
                    return null;
                }
                if (_board == null)
                {
                    _board = SessionManagerOld.Instance.Board;
                }

                return _board;
            }
            private set => _board = value;
        }
        
        private evolute_duel_Board _boardModel;
        private evolute_duel_Board BoardModel => GetBoardModel();

        public TileData CurrentTile { get; private set; }
        public List<ValidPlacement> CurrentValidPlacements { get; private set; }
        public Dictionary<ValidPlacement, TileData> CurrentJokers { get; private set; }
        
        public BotDataCollectorModule(Bot bot) : base(bot)
        {

        }
        
        public void CollectData()
        {
            if (Board == null || BoardModel == null)
            {
                CustomLogger.LogWarning("BotDataCollectorModule: BoardModel or Board is null");
                return;
            }
            CurrentTile = new TileData(OnChainBoardDataConverter.GetTopTile(BoardModel.top_tile));
            CurrentValidPlacements = Board.GetValidPlacements(CurrentTile);
        }

        public void CollectJokerData()
        {
            if (Board == null || BoardModel == null)
            {
                CustomLogger.LogWarning("BotDataCollectorModule: BoardModel or Board is null");
                return;
            }
            
            List<ValidPlacement> jokerPlacements = Board.GetJokerValidPlacements();
            CurrentJokers = new Dictionary<ValidPlacement, TileData>();
            
            foreach (var jokerPlacement in jokerPlacements)
            {
                Vector2Int position = new Vector2Int(jokerPlacement.x, jokerPlacement.y);
                TileData jokerTile = JokerManager.GetOneJokerCombination(position.x, position.y);
                CurrentJokers.Add(new ValidPlacement(position), jokerTile); 
            }

        }
        
        // TODO: Need to create a new class for getting models
        public evolute_duel_Board GetBoardModel()
        {
            if (_boardModel != null)
            {
                return _boardModel;
            }
            string address = Bot.Account.Address.Hex();
            GameObject[] boardsGO = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Board>();
            foreach (var boardGO in boardsGO)
            {
                if (boardGO.TryGetComponent(out evolute_duel_Board board))
                {
                    if (board.player1.Item1.Hex() == address || board.player2.Item1.Hex() == address)
                    {
                        return board;
                    }
                }
            }
            return null;
        }
        
    }
}