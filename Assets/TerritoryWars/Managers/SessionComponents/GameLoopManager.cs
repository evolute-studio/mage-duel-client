using System;
using System.Threading.Tasks;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class GameLoopManager: ISessionComponent
    {
        private SessionManagerContext _managerContext;
        private SessionContext _sessionContext => _managerContext.SessionContext;
        private TurnEndData _turnEndData = new TurnEndData();
        
        private Player _currentPlayer
        {
            get { return _sessionContext.CurrentTurnPlayer; }
            set { _sessionContext.CurrentTurnPlayer = value; }
        }

        private Player _localPlayer => _sessionContext.LocalPlayer;
        private Player _remotePlayer => _sessionContext.RemotePlayer;

        public void Initialize(SessionManagerContext managerContext)
        {
            _managerContext = managerContext;
            EventBus.Subscribe<TurnEndData>(OnTurnEnd);
            EventBus.Subscribe<BoardUpdated>(BoardUpdate);
            EventBus.Subscribe<Moved>(Moved);
            EventBus.Subscribe<Skipped>(Skipped);
            EventBus.Subscribe<ClientInput>(OnLocalFinishTurn);
        }

        public async void StartGame()
        {
            Board board = _sessionContext.Board;
            if(board.IsNull)
            {
                CustomLogger.LogError("GameLoopManager: Board is null or not initialized.");
                return;
            }
            byte whoseTurnSide = await WhoseTurn();
            //byte convertedSide = (byte)SetLocalPlayerData.GetLocalIndex(whoseTurnSide);
            _currentPlayer = _sessionContext.Players[whoseTurnSide];
            
            StartTurn();
        }

        private void StartTurn()
        {
            CustomLogger.LogImportant("StartTurn");
            CustomLogger.LogImportant($"Current player: {_currentPlayer.PlayerId} ({_currentPlayer.PlayerSide})");
            CustomLogger.LogImportant($"Local player: {_localPlayer.PlayerId} ({_localPlayer.PlayerSide})");
            CustomLogger.LogImportant($"Remote player: {_remotePlayer.PlayerId} ({_remotePlayer.PlayerSide})");
            if(_currentPlayer == _localPlayer) StartLocalTurn();
            if(_currentPlayer == _remotePlayer) StartRemoteTurn();
        }

        private void StartLocalTurn()
        {
            _localPlayer.StartSelectingAnimation();
            string currentTile = _sessionContext.Board.TopTile;
            CustomLogger.LogImportant($"Current tile: {currentTile}");
            TileData tileData = new TileData(currentTile, Vector2Int.zero, _localPlayer.PlayerSide);
            _managerContext.TileSelector.StartTilePlacement(tileData);
        }

        private void BoardUpdate(BoardUpdated data)
        {
            _turnEndData.SetBoardUpdated(ref data);
            _sessionContext.Board.SetData(data);
            _sessionContext.Board.Player1.Update(data.Player1);
            _sessionContext.Board.Player2.Update(data.Player2);
            _sessionContext.PlayersData[0].Update(data.Player1);
            _sessionContext.PlayersData[1].Update(data.Player2);
            _sessionContext.Players[0].SetData(_sessionContext.Board.Player1);
            _sessionContext.Players[1].SetData(_sessionContext.Board.Player2);
        }

        private void Moved(Moved data)
        {
            _turnEndData.SetMoved(ref data);
            if(data.PlayerId == _localPlayer.PlayerId) return;
            TileData tileData = new TileData(data.tileModel);
            _managerContext.BoardManager.PlaceTile(tileData);
        }

        private void Skipped(Skipped data)
        {
            _turnEndData.SetSkipped(ref data);
            if(data.PlayerId == _localPlayer.PlayerId) return;
            _currentPlayer.PlaySkippedBubbleAnimation();
        }

        private void OnLocalFinishTurn(ClientInput input)
        {
            CustomLogger.LogImportant("OnLocalFinishTurn");
            switch (input.Type)
            {
                case ClientInput.InputType.Skip: SkipLocalTurn(); break;
                case ClientInput.InputType.Move: FinishLocalTurn(); break;
            }
        }

        private void FinishLocalTurn()
        {
            CustomLogger.LogImportant("FinishLocalTurn");
            CustomLogger.LogImportant($"Current player: {_currentPlayer.PlayerId} ({_currentPlayer.PlayerSide})");
            CustomLogger.LogImportant($"Current tile: {_managerContext.TileSelector.CurrentTile}");
            if (_managerContext.TileSelector.CurrentTile != null && _currentPlayer == _localPlayer)
            {
                _managerContext.TileSelector.PlaceCurrentTile();
            }
        }

        private void SkipLocalTurn()
        {
            if(_currentPlayer != _localPlayer) return;
            GameUI.Instance.SetJokerMode(false);    
            _managerContext.TileSelector.EndTilePlacement();
        }

        private void StartRemoteTurn()
        {
            CustomLogger.LogImportant("StartRemoteTurn");
            _remotePlayer.StartSelectingAnimation();
        }
        
        public void OnTurnEnd(TurnEndData turnEndData)
        {
            _currentPlayer.EndTurnAnimation();
            _turnEndData.Reset();
            byte nextTurnSide = (byte)((_currentPlayer.PlayerSide + 1) % 2);
            _currentPlayer = _sessionContext.Players[nextTurnSide];
            StartTurn();
        }
        
        public void EndGame() { }
        
        
        private async Task<byte> WhoseTurn()
        {
            Board board = _sessionContext.Board;
            string lastMoveId = board.LastMoveId;
            byte lastSide = 1;
            ulong lastTimestamp = board.LastUpdateTimestamp;
            if (!String.IsNullOrEmpty(board.LastMoveId))
            {
                Move lastMove = await DojoLayer.Instance.GetMove(lastMoveId);
                lastSide = lastMove.PlayerSide;
                lastTimestamp = lastMove.Timestamp;
            }
            ushort turnCountElapsed = GetTurnCount(lastTimestamp);
            byte currentTurnSide = (byte)((lastSide + turnCountElapsed + 1) % 2);
            return currentTurnSide;
        }

        private ushort GetTurnCount(ulong lastTimestamp)
        {
            ushort moveDuration = GameConfiguration.TurnDuration;
            ulong currentTimestamp = (ulong) (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            if (lastTimestamp == 0)
            {
                return 0;
            }
            ulong elapsedTime = currentTimestamp - lastTimestamp;
            return (ushort)(elapsedTime / moveDuration);
        }

        public void Dispose()
        {
            EventBus.Unsubscribe<TurnEndData>(OnTurnEnd);
            EventBus.Unsubscribe<BoardUpdated>(BoardUpdate);
            EventBus.Unsubscribe<Moved>(Moved);
            EventBus.Unsubscribe<Skipped>(Skipped);
            EventBus.Unsubscribe<ClientInput>(OnLocalFinishTurn);
        }
    }

    public class TurnEndData
    {
        public BoardUpdated BoardUpdated;
        public Moved Moved;
        public Skipped Skipped;
        

        public void SetBoardUpdated(ref BoardUpdated boardUpdated)
        {
            BoardUpdated = boardUpdated;
            IsTurnEnded();
        }
        
        public void SetMoved(ref Moved moved)
        {
            Moved = moved;
            IsTurnEnded();
        }
        
        public void SetSkipped(ref Skipped skipped)
        {
            Skipped = skipped;
            IsTurnEnded();
        }
        
        public void Reset()
        {
            BoardUpdated = default;
            Moved = default;
            Skipped = default;
        }

        public void IsTurnEnded()
        {
            if (!BoardUpdated.IsNull && (Moved.IsNull || Skipped.IsNull))
            {
                EventBus.Publish(this);
                Reset();
            }
        }
    }
}
