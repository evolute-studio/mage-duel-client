using System.Collections.Generic;
using TerritoryWars.DataModels;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Managers.SessionComponents;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using UnityEngine;
using Vector2Int = UnityEngine.Vector2Int;

namespace TerritoryWars.Bots
{
    public class BotDataCollectorModule : BotModule
    {
        public override Bot Bot { get; set; }
        private int _botPlayerSide => _sessionContext.GetPlayerById(Bot.Account.Address.Hex()).PlayerSide;

        private BoardManager _board;

        public BoardManager Board
        {
            get
            {
                if (SessionManager.Instance == null)
                {
                    CustomLogger.LogWarning("BotDataCollectorModule: SessionManager is null");
                    return null;
                }
                if (_board == null)
                {
                    _board = SessionManager.Instance.BoardManager;
                }

                return _board;
            }
            private set => _board = value;
        }

        private SessionContext _sessionContext => DojoGameManager.Instance.GlobalContext.SessionContext;

        public TileData CurrentTile { get; private set; }
        public List<ValidPlacement> CurrentValidPlacements { get; private set; }
        public Dictionary<ValidPlacement, TileData> CurrentJokers { get; private set; }

        public BotDataCollectorModule(Bot bot) : base(bot)
        {

        }

        public void CollectData()
        {
            CurrentTile = new TileData(_sessionContext.Board.TopTile, new Vector2Int(-1, -1), _botPlayerSide);
            CurrentValidPlacements = Board.GetValidPlacements(CurrentTile);
            //SessionManager.Instance.TileSelector.ShowPossiblePlacements(CurrentValidPlacements);
        }

        public void CollectJokerData()
        {
            List<ValidPlacement> jokerPlacements = Board.GetJokerValidPlacements();
            CurrentJokers = new Dictionary<ValidPlacement, TileData>();

            foreach (var jokerPlacement in jokerPlacements)
            {
                Vector2Int position = new Vector2Int(jokerPlacement.x, jokerPlacement.y);
                TileData jokerTile = JokerManager.GetOneJokerCombination(position.x, position.y);
                CurrentJokers.Add(new ValidPlacement(position), jokerTile);
            }

        }

    }
}
