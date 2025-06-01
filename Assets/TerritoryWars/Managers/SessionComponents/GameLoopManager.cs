using System;
using System.Threading.Tasks;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.ClientEvents;
using TerritoryWars.DataModels.Events;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
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
            EventBus.Subscribe<BoardUpdated>(OnBoardUpdate);
            EventBus.Subscribe<Moved>(OnMoved);
            EventBus.Subscribe<Skipped>(OnSkipped);
            EventBus.Subscribe<ClientInput>(OnLocalFinishTurn);
            EventBus.Subscribe<TimerEvent>(OnTimerEvent);
            EventBus.Subscribe<TurnEndData>(OnTurnEnd);
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
            CustomLogger.LogDojoLoop("StartTurn");
            EventBus.Publish(new TimerEvent(TimerEventType.Started, GetTimerTimestamp()));
            
            string currentTile = _sessionContext.Board.TopTile;
            TileData tileData = new TileData(currentTile, Vector2Int.zero, _localPlayer.PlayerSide);
            _managerContext.TileSelector.tilePreview.UpdatePreview(tileData);
            
            if(_currentPlayer == _localPlayer) StartLocalTurn();
            if(_currentPlayer == _remotePlayer) StartRemoteTurn();
        }

        private void StartLocalTurn()
        {
            CustomLogger.LogDojoLoop("StartLocalTurn");
            _localPlayer.StartSelectingAnimation();
            string currentTile = _sessionContext.Board.TopTile;
            TileData tileData = new TileData(currentTile, Vector2Int.zero, _localPlayer.PlayerSide);
            _managerContext.TileSelector.StartTilePlacement(tileData);
            
            GameUI.Instance.SetEndTurnButtonActive(false);
            GameUI.Instance.SetRotateButtonActive(false);
            GameUI.Instance.SetSkipTurnButtonActive(true);

            if (_sessionContext.IsGameWithBotAsPlayer)
            {
                DojoGameManager.Instance.LocalBotAsPlayer.MakeMove();
            }
        }
        
        private void StartRemoteTurn()
        {
            CustomLogger.LogDojoLoop("StartRemoteTurn");
            _remotePlayer.StartSelectingAnimation();
            
            if (_sessionContext.IsGameWithBot || _sessionContext.IsGameWithBotAsPlayer)
            {
                DojoGameManager.Instance.LocalBot.MakeMove();
            }
        }

        private void OnBoardUpdate(BoardUpdated data)
        {
            _sessionContext.Board.SetData(data);
            _sessionContext.Board.Player1.Update(data.Player1);
            _sessionContext.Board.Player2.Update(data.Player2);
            _sessionContext.PlayersData[0].Update(data.Player1);
            _sessionContext.PlayersData[1].Update(data.Player2);
            _sessionContext.Players[0].SetData(_sessionContext.Board.Player1);
            _sessionContext.Players[1].SetData(_sessionContext.Board.Player2);
            GameUI.Instance.playerInfoUI.UpdateData(_sessionContext.PlayersData);
            _turnEndData.SetBoardUpdated(ref data);
        }

        private void OnMoved(Moved data)
        {
            if (data.PlayerId != _localPlayer.PlayerId)
            {
                 TileData tileData = new TileData(data.tileModel);
                _managerContext.BoardManager.PlaceTile(tileData); 
            }

            _sessionContext.Board.UpdateTimestamp(data.Timestamp);
            _turnEndData.SetMoved(ref data);
        }

        private void OnSkipped(Skipped data)
        {
            if (data.PlayerId != _localPlayer.PlayerId)
            {
                _currentPlayer.PlaySkippedBubbleAnimation();
            } 
            
            _sessionContext.Board.UpdateTimestamp(data.Timestamp);
            _turnEndData.SetSkipped(ref data);
        }

        private void OnLocalFinishTurn(ClientInput input)
        {
            
            switch (input.Type)
            {
                case ClientInput.InputType.Skip: SkipLocalTurn(); break;
                case ClientInput.InputType.Move: FinishLocalTurn(); break;
            }
        }

        private void OnTimerEvent(TimerEvent timerEvent)
        {
            if (timerEvent.Type == TimerEventType.TurnTimeElapsed && _sessionContext.IsLocalPlayerTurn)
            {
                SkipLocalTurn();
            }
            else if (timerEvent.Type == TimerEventType.PassingTimeElapsed && !_sessionContext.IsLocalPlayerTurn)
            {
                SkipOpponentTurnLocally();
            }
        }

        private void FinishLocalTurn()
        {
          
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
            DojoConnector.SkipMove(DojoGameManager.Instance.LocalAccount);
        }

        private void SkipOpponentTurnLocally()
        {
            if(_currentPlayer != _remotePlayer) return;
            string id = "0x0";
            string playerId = _remotePlayer.PlayerId;
            string prevMoveId = _sessionContext.Board.LastMoveId;
            string boardId = _sessionContext.Board.Id;
            ulong timestamp = (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            Skipped skipped = new Skipped(id, playerId, prevMoveId, boardId, timestamp);
            OnSkipped(skipped);
            BoardUpdated data = new BoardUpdated().SetData(_sessionContext.Board);
            OnBoardUpdate(data);
        }

        
        
        public void OnTurnEnd(TurnEndData turnEndData)
        {
            CustomLogger.LogDojoLoop("OnTurnEnd");
            _managerContext.TileSelector.tilePreview.ResetPosition();
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

        private ulong GetTimerTimestamp()
        {
            ulong wholeTurnDuration = (ulong)GameConfiguration.TurnDuration * GameConfiguration.PassingTurnDuration;
            ulong currentTimestamp = (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            ulong lastTimestamp = _sessionContext.Board.LastUpdateTimestamp;
            ulong delta = currentTimestamp - lastTimestamp;
            return lastTimestamp + wholeTurnDuration * (delta / wholeTurnDuration);
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
            EventBus.Unsubscribe<BoardUpdated>(OnBoardUpdate);
            EventBus.Unsubscribe<Moved>(OnMoved);
            EventBus.Unsubscribe<Skipped>(OnSkipped);
            EventBus.Unsubscribe<ClientInput>(OnLocalFinishTurn);
            EventBus.Unsubscribe<TimerEvent>(OnTimerEvent);
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
            if (!BoardUpdated.IsNull && (!Moved.IsNull || !Skipped.IsNull))
            {
                EventBus.Publish(this);
                Reset();
            }
        }
    }
}
