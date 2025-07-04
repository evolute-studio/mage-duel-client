using System;
using System.Collections;
using System.Threading.Tasks;
using Dojo.Starknet;
using TerritoryWars.ConnectorLayers.Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.ClientEvents;
using TerritoryWars.DataModels.Events;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.SaveStorage;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using TerritoryWars.UI.Popups;
using TerritoryWars.UI.Session;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class SpectatorLoopManager : LoopManager, ISessionComponent
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
        
        private PhaseStarted _currentPhase;

        public override void Initialize(SessionManagerContext managerContext)
        {
            _managerContext = managerContext;
            EventBus.Subscribe<BoardUpdated>(OnBoardUpdate);
            EventBus.Subscribe<Moved>(OnMoved);
            EventBus.Subscribe<Skipped>(OnSkipped);
            EventBus.Subscribe<ClientInput>(OnLocalFinishTurn);
            EventBus.Subscribe<TimerEvent>(OnTimerEvent);
            EventBus.Subscribe<UnionFind>(OnUnionFind);
            EventBus.Subscribe<GameFinished>(OnEndGame);
            EventBus.Subscribe<ErrorOccured>(OnError);
            EventBus.Subscribe<GameCanceled>(OnGameCanceled);
            EventBus.Subscribe<PhaseStarted>(OnPhaseStarted);
            EventBus.Subscribe<TilePlaced>(OnTilePlaced);
            EventBus.Subscribe<TurnEndData>(OnTurnEnd);
        }
        
        public override async void StartGame()
        {
            byte whoseTurnSide = await WhoseTurn();
            _currentPlayer = _sessionContext.Players[whoseTurnSide];
            
            GameUI.Instance.SetEndTurnButtonActive(false);
            GameUI.Instance.SetRotateButtonActive(false);
            GameUI.Instance.SetSkipTurnButtonActive(false);
            GameUI.Instance.SetSpectatingDeckContainer(true);
            
            PhaseStarted phaseStarted = new PhaseStarted().SetData(_sessionContext.Board);
            _currentPhase = phaseStarted;
            OnPhaseStarted(phaseStarted);
        }
        
        private void OnPhaseStarted(PhaseStarted phaseStarted)
        {
            CustomLogger.LogObject(phaseStarted, "OnPhaseStarted");
            
            if (phaseStarted.Phase == SessionPhase.Reveal)
            {
                // if it's session start or game creation - invoke reveal action here
                if (_currentPhase.IsNull || _currentPhase.Phase == SessionPhase.Creating)
                {
                    OnRevealPhase(phaseStarted);
                    _sessionContext.Board.SetData(_currentPhase);
                    _currentPhase = phaseStarted;
                }
                else
                {
                    // in this case the reveal phase started is move phase ended and this phase a part of turn end
                    _turnEndData.OnPhaseStarted(ref phaseStarted);
                    _currentPhase = phaseStarted;
                }
                return;
            }
            else if (phaseStarted.Phase == SessionPhase.Move)
            {
                // if it's session start or game creation - invoke reveal action here
                if (!_currentPhase.IsNull && _currentPhase.Phase == SessionPhase.Move)
                {
                    _turnEndData.OnPhaseStarted(ref phaseStarted);
                    _currentPhase = phaseStarted;
                }
                else
                {
                    // in this case the reveal phase started is move phase ended and this phase a part of turn end
                    OnMovePhase(phaseStarted);
                    _sessionContext.Board.SetData(_currentPhase);
                    _currentPhase = phaseStarted;
                }
                return;
            }
            
            switch (phaseStarted.Phase)
            {
                case SessionPhase.Creating:
                    OnGameCreationPhase();
                    break;
                case SessionPhase.Request:
                    OnRequestPhase(phaseStarted);
                    break;
                case SessionPhase.Move:
                    OnMovePhase(phaseStarted);
                    break;
                case SessionPhase.Finished:
                    break;
            }
            _currentPhase = phaseStarted;
            _sessionContext.Board.SetData(_currentPhase);
        }
        
        private void OnRequestPhase(PhaseStarted phaseData)
        {
            EventBus.Publish(new TimerEvent(TimerEventType.Requesting, TimerProgressType.Started, GetPhaseStart()));
            
            /*
            // if local player turn - wait for remote player to request tile
            // if remote player turn - request tile
            
            CommitmentsData commitments;
            GeneralAccount account;
            if (_sessionContext.IsGameWithBot)
            {
                if (_sessionContext.IsLocalPlayerTurn)
                {
                    commitments = _sessionContext.BotCommitments;
                    account = DojoGameManager.Instance.LocalBot.Account;
                }
                else
                {
                    commitments = _sessionContext.Commitments;
                    account = DojoGameManager.Instance.LocalAccount;
                }
            }
            else
            {
                if (_sessionContext.IsLocalPlayerTurn)
                {
                    return;
                }
                else
                {
                    commitments = _sessionContext.Commitments;
                    account = DojoGameManager.Instance.LocalAccount;
                }
            }
            
            if (!phaseData.TopTileIndex.HasValue)
            {
                CustomLogger.LogError("GameLoopManager: CommitedTileIndex is null in OnRequestPhase.");
                return;
            }

            byte commitedTileIndex = phaseData.TopTileIndex.Value;*/
        }
        
        private void OnGameCreationPhase()
        {
            EventBus.Publish(new TimerEvent(TimerEventType.GameCreation, TimerProgressType.Started, GetPhaseStart()));

            CommitmentsData commitmentsData = _sessionContext.Commitments;
            uint[] hashes = commitmentsData.GetAllHashes();
            DojoConnector.CommitTiles(DojoGameManager.Instance.LocalAccount, hashes);
            CustomLogger.LogDojoLoop("OnGameCreationPhase: Local player - sending commitments to server.");

            if (_sessionContext.IsGameWithBot)
            {
                commitmentsData = _sessionContext.BotCommitments;
                hashes = commitmentsData.GetAllHashes();
                DojoConnector.CommitTiles(DojoGameManager.Instance.LocalBot.Account, hashes);
                CustomLogger.LogDojoLoop("OnGameCreationPhase: Bot player - sending commitments to server.");
            }
        }
        
        private void OnRevealPhase(PhaseStarted phaseData)
        {
            EventBus.Publish(new TimerEvent(TimerEventType.Revealing, TimerProgressType.Started, GetPhaseStart()));

            /*CommitmentsData commitments;
            GeneralAccount account;
            if (_sessionContext.IsGameWithBot)
            {
                if (_sessionContext.IsLocalPlayerTurn)
                {
                    commitments = _sessionContext.Commitments;
                    account = DojoGameManager.Instance.LocalAccount;
                }
                else
                {
                    commitments = _sessionContext.BotCommitments;
                    account = DojoGameManager.Instance.LocalBot.Account;
                }
            }
            else
            {
                if (_sessionContext.IsLocalPlayerTurn)
                {
                    commitments = _sessionContext.Commitments;
                    account = DojoGameManager.Instance.LocalAccount;
                }
                else
                {
                    return;
                }
            }

            if (!phaseData.CommitedTile.HasValue)
            {
                CustomLogger.LogError("GameLoopManager: CommitedTileIndex is null in OnRevealPhase.");
                return;
            }*/

            // byte commitedTileIndex = commitments.GetIndex(phaseData.CommitedTile.Value);
            // FieldElement nonce = commitments.Nonce[commitedTileIndex];
            // byte c = commitments.Permutations[commitedTileIndex];
            // CustomLogger.LogDojoLoop(
            //     $"CommitedTile: {phaseData.CommitedTile.Value} Index: {commitedTileIndex}, NonceHex: {nonce.Hex()}, C: {c}");
            // DojoConnector.RevealTile(account, commitedTileIndex, nonce, c);
        }

        private void OnMovePhase(PhaseStarted phaseStarted)
        {
            _sessionContext.Board.TopTileIndex = phaseStarted.TopTileIndex;

            EventBus.Publish(new TimerEvent(TimerEventType.Moving, TimerProgressType.Started, GetPhaseStart()));
            
            StartMoving();

            /*// regular case
            if (phaseStarted.TopTileIndex.HasValue && phaseStarted.CommitedTile.HasValue)
            {
                /*if (!_sessionContext.IsLocalPlayerTurn)
                {
                    byte commitedTile = _sessionContext.Commitments.GetIndex(phaseStarted.CommitedTile.Value, false);
                    // byte c = _sessionContext.Commitments.Permutations[commitedTile];
                    // string tileType = _sessionContext.Board.AvailableTilesInDeck[commitedTile];
                    // TileData tileData = new TileData(tileType, Vector2Int.zero, _localPlayer.PlayerSide);

                    GameUI.Instance.ShowNextTileAnimation.DropCurrentTile(() =>
                    {
                        ShowCurrentTile();
                        GameUI.Instance.ShowNextTileAnimation.NextTileFogReveal(() =>
                        {
                            StartMoving();
                            GameUI.Instance.ShowNextTileActive(true, null, tileData);
                        });

                    });

                }
                else
                {
                    GameUI.Instance.ShowNextTileAnimation.DropCurrentTile(() =>
                    {
                        GameUI.Instance.ShowNextTileActive(false, () =>
                        {
                            GameUI.Instance.ShowNextTileAnimation.ActivateBackground(true, false);
                            StartMoving();
                        });
                    });
                }
            }
            // case when in deck there is no tiles left, but we have top tile
            else if (phaseStarted.TopTileIndex.HasValue)
            {
                if (_sessionContext.IsLocalPlayerTurn)
                {
                    GameUI.Instance.ShowNextTileActive(false, StartMoving);
                }
                else
                {
                    StartMoving();
                }

            }
            // case when there is no top tile and no commited tile. But we can still use joker or skip
            else
            {
                if (!_sessionContext.IsLocalPlayerTurn)
                {
                    StartMoving();
                }
                else
                {
                    if (_currentPlayer.JokerCount > 0) StartMoving();
                    else SkipLocalTurn();
                }
            }
            */

        }



        private void FinishGame()
        {
            DojoConnector.FinishGame(DojoGameManager.Instance.LocalAccount, new FieldElement(_sessionContext.Board.Id));
        }

        private void StartMoving()
        {
            GameUI.Instance.InitialDeckContainerActivation();

            ShowCurrentTile();
            CustomLogger.LogDojoLoop("StartTurn");

            StartRemoteTurn();
        }

        public void ShowCurrentTile()
        {
            TileData currentTile = GetCurrentTile();
            if (currentTile == null)
            {
                _managerContext.TileSelector.tilePreview.SetActivePreview(_sessionContext.IsLocalPlayerTurn);
                //_managerContext.TileSelector.tilePreview.UpdatePreview(null);
            }
            else
            {
                _managerContext.TileSelector.tilePreview.UpdatePreview(currentTile);
            }
        }

        public TileData GetCurrentTile()
        {
            _managerContext.TileSelector.tilePreview.SetActivePreview(true);
            string currentTile = _sessionContext.Board.TopTile;
            if (currentTile == null)
            {
                return null;
            }
            else
            {
                TileData tileData = new TileData(currentTile, Vector2Int.zero, _currentPlayer.PlayerSide);
                return tileData;
            }
        }

        private void StartLocalTurn()
        {
            CustomLogger.LogDojoLoop("StartLocalTurn");
            _localPlayer.StartSelectingAnimation();
            string currentTile = _sessionContext.Board.TopTile;
            if (currentTile == null)
            {
                TryJoker();
            }
            else
            {
                TileData tileData = new TileData(currentTile, Vector2Int.zero, _localPlayer.PlayerSide);
                _managerContext.TileSelector.StartTilePlacement(tileData);

                if (_managerContext.TileSelector.IsExistValidPlacement(tileData))
                {
                    _managerContext.TileSelector.StartTilePlacement(tileData);
                }
                else
                {
                    TryJoker();
                }

                if (_sessionContext.IsGameWithBotAsPlayer)
                {
                    DojoGameManager.Instance.LocalBotAsPlayer.MakeMove();
                }
            }


            GameUI.Instance.SetEndTurnButtonActive(false);
            GameUI.Instance.SetRotateButtonActive(false);
            GameUI.Instance.SetSkipTurnButtonActive(true);
            GameUI.Instance.SetActiveDeckContainer(true);



            void TryJoker()
            {
                if (_currentPlayer.JokerCount > 0)
                {
                    GameUI.Instance.OnJokerButtonClicked();
                }
                else
                {
                    GameUI.Instance.SetActiveSkipButtonPulse(true);
                }
            }
        }
        private void StartRemoteTurn()
        {
            CustomLogger.LogDojoLoop("StartRemoteTurn");
            _remotePlayer.StartSelectingAnimation();
            
            GameUI.Instance.SetEndTurnButtonActive(false);
            GameUI.Instance.SetRotateButtonActive(false);
            GameUI.Instance.SetSkipTurnButtonActive(false);
            GameUI.Instance.SetActiveDeckContainer(false);

            if (_sessionContext.IsGameWithBot || _sessionContext.IsGameWithBotAsPlayer)
            {
                if (_managerContext.SessionManager.IsBotSkipping)
                {
                    DojoGameManager.Instance.LocalBot.LogicModule.SkipMove();
                    return;
                }
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
            GameUI.Instance.playerInfoUI.SetDeckCount(_sessionContext.Board.GetTilesInDeck());
            _turnEndData.SetBoardUpdated(ref data);
            _managerContext.JokerManager.SetJokersCount(0, SessionManager.Instance.SessionContext.Board.Player1.JokerCount);
            _managerContext.JokerManager.SetJokersCount(1, SessionManager.Instance.SessionContext.Board.Player2.JokerCount);
        }

        private void OnMoved(Moved data)
        {
            TileData tileData = new TileData(data.tileModel);
            //_managerContext.BoardManager.PlaceTile(tileData);

            Coroutines.StartRoutine(HandleOpponentMoveCoroutine(tileData));
            

            _sessionContext.Board.UpdateTimestamp(data.Timestamp);
            _turnEndData.SetMoved(ref data);
        }

        private void OnTilePlaced(TilePlaced data)
        {
            GameUI.Instance.ShowNextTileAnimation.ShiftCurrentTile();
        }
        
        private IEnumerator HandleOpponentMoveCoroutine(TileData tile)
        {
            _managerContext.TileSelector.SetCurrentTile(tile);
            _managerContext.TileSelector.tilePreview.SetPosition(tile.Position);
            yield return new WaitForSeconds(0.3f);
            _managerContext.TileSelector.tilePreview.PlaceTile(tile, () =>
            {
                _managerContext.BoardManager.PlaceTile(tile);
                _turnEndData.OnTilePlaced();
            });
            yield return new WaitForSeconds(0.5f);
            _currentPlayer.EndTurnAnimation();
            yield return new WaitForSeconds(0.5f);
            _managerContext.TileSelector.tilePreview.ResetPosition();
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

        private void OnUnionFind(UnionFind unionFind)
        {
            _sessionContext.UnionFind = unionFind;
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
            // if (timerEvent.Type == TimerEventType.Moving && timerEvent.ProgressType == TimerProgressType.Elapsed 
            //                                              && _sessionContext.IsLocalPlayerTurn)
            // {
            //     SkipLocalTurn();
            // }
            // else if (timerEvent.ProgressType == TimerProgressType.Elapsed)
            // {
            //     FinishGame();
            // }
            // else if (timerEvent.Type == TimerEventType.PassingTimeElapsed && !_sessionContext.IsLocalPlayerTurn)
            // {
            //     SkipOpponentTurnLocally();
            // }
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
            if (_currentPlayer != _localPlayer) return;
            GameUI.Instance.SetJokerMode(false);
            _managerContext.TileSelector.EndTilePlacement();
            DojoConnector.SkipMove(DojoGameManager.Instance.LocalAccount);
        }
        
        public void OnTurnEnd(TurnEndData turnEndData)
        {
            CustomLogger.LogDojoLoop("OnTurnEnd");
            _currentPlayer.EndTurnAnimation();
            byte nextTurnSide = (byte)((_currentPlayer.PlayerSide + 1) % 2);
            _currentPlayer = _sessionContext.Players[nextTurnSide];

            PhaseStarted phaseStarted = _turnEndData.PhaseStarted;
            turnEndData.Reset();

            if (!phaseStarted.IsNull && phaseStarted.Phase == SessionPhase.Reveal)
            {
                OnRevealPhase(phaseStarted);
                _sessionContext.Board.SetData(phaseStarted);
            }
            else if (!phaseStarted.IsNull && phaseStarted.Phase == SessionPhase.Move)
            {
                OnMovePhase(phaseStarted);
                _sessionContext.Board.SetData(phaseStarted);
            }
            else
                CustomLogger.LogError($"GameLoopManager: PhaseStarted is null {phaseStarted.IsNull} or not Reveal phase " +
                                      $"in OnTurnEnd {phaseStarted.Phase != SessionPhase.Reveal}.");
        }

        public void OnEndGame(GameFinished gameFinished)
        {
            _managerContext.ContestManager.ContestProcessor.SetGameFinished(true);
            SessionManager.Instance.StructureHoverManager.IsGameFinished = true;
            CustomLogger.LogExecution($"[GameFinished]");
            FinishGameContests.FinishGameAction = () =>
            {
                Coroutines.StartRoutine(GameFinishedDelayed());
            };
        }

        public void OnError(ErrorOccured error)
        {
            var errorType = error.ErrorType;
            switch (errorType)
            {
                case ServerErrorType.InvalidMove:
                    PopupManager.Instance.ShowInvalidMovePopup();
                    break;
                case ServerErrorType.NotYourTurn:
                    PopupManager.Instance.NotYourTurnPopup();
                    break;
            }

            CustomLogger.LogError($"[{errorType}] | Player: {error.Player}");
        }
        
        private void OnGameCanceled(GameCanceled gameCanceled)
        {
            SimpleStorage.ClearCurrentBoardId();
            PopupManager.Instance.ShowOpponentCancelGame();
        }
        
        

        private IEnumerator GameFinishedDelayed()
        {
            yield return new WaitForSeconds(2f);
            SimpleStorage.ClearCurrentBoardId();
            GameUI.Instance.ShowResultPopUp();
        }
        

        private bool IsGameStillActual()
        {
            Board board = _sessionContext.Board;
            int phaseDuration = GameConfiguration.GetPhaseDuration(board.Phase);
            ulong phaseStartedAt = board.PhaseStartedAt;
            ulong currentTimestamp = (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            if (phaseStartedAt == 0 || currentTimestamp < phaseStartedAt)
            {
                CustomLogger.LogError("GameLoopManager: Phase started at is not set or in the past.");
                return false;
            }
            int elapsedTime = (int)currentTimestamp - (int)phaseStartedAt;
            if (elapsedTime > phaseDuration)
            {
                return false;
            }
            return true;
        }

        public override async Task<byte> WhoseTurn()
        {
            Board board = _sessionContext.Board;
            string lastMoveId = board.LastMoveId;
            byte lastSide = 1;
            if (!String.IsNullOrEmpty(board.LastMoveId))
            {
                Move lastMove = await DojoModels.GetMove(lastMoveId);
                lastSide = lastMove.PlayerSide;
            }
            byte currentTurnSide = (byte)((lastSide + 1) % 2);
            return currentTurnSide;
        }

        private ulong GetTimerTimestamp()
        {
            ulong wholeTurnDuration = (ulong)GameConfiguration.TurnDuration + GameConfiguration.PassingTurnDuration;
            ulong currentTimestamp = (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            ulong lastTimestamp = _sessionContext.Board.LastUpdateTimestamp;
            ulong delta = currentTimestamp - lastTimestamp;
            return lastTimestamp + wholeTurnDuration * (delta / wholeTurnDuration);
        }

        private ulong GetPhaseStart() => _sessionContext.Board.PhaseStartedAt;
        

        public void Dispose()
        {
            EventBus.Unsubscribe<TurnEndData>(OnTurnEnd);
            EventBus.Unsubscribe<BoardUpdated>(OnBoardUpdate);
            EventBus.Unsubscribe<Moved>(OnMoved);
            EventBus.Unsubscribe<Skipped>(OnSkipped);
            EventBus.Unsubscribe<ClientInput>(OnLocalFinishTurn);
            EventBus.Unsubscribe<UnionFind>(OnUnionFind);
            EventBus.Unsubscribe<TimerEvent>(OnTimerEvent);
            EventBus.Unsubscribe<GameFinished>(OnEndGame);
            EventBus.Unsubscribe<ErrorOccured>(OnError);
            EventBus.Unsubscribe<GameCanceled>(OnGameCanceled);
            EventBus.Unsubscribe<PhaseStarted>(OnPhaseStarted);
            EventBus.Unsubscribe<TilePlaced>(OnTilePlaced);
        }
    }
}