using Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;
using TerritoryWars.Dojo;
using TerritoryWars.Managers;
using TerritoryWars.Tools;

namespace TerritoryWars.General
{
    public class EventsHandler
    {
        private WorldManager _worldManager;
        private GlobalContext _globalContext => DojoGameManager.Instance.GlobalContext;

        public EventsHandler(WorldManager worldManager)
        {
            _worldManager = worldManager;
            _worldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
            //_worldManager.synchronizationMaster.OnModelUpdated.AddListener(OnModelUpdated);
            IncomingModelsFilter.OnModelPassed.AddListener(OnModelUpdated);
        }

        private void OnEventMessage(ModelInstance modelInstance)
        {
            CustomLogger.LogEventsAll($"[EventHandler] | {modelInstance.Model.Name} ");
            switch (ApplicationState.CurrentState)
            {
                case ApplicationStates.Initializing:
                    break;
                case ApplicationStates.Leaderboard:
                    break;
                case ApplicationStates.MatchTab:
                    break;
                case ApplicationStates.Menu:
                    MenuEventHandler(modelInstance);
                    break;
                case ApplicationStates.Session:
                    SessionEventHandler(modelInstance);
                    break;
                case ApplicationStates.SnapshotTab:
                    break;
            }
        }

        private void OnModelUpdated(ModelInstance modelInstance)
        {
            switch (ApplicationState.CurrentState)
            {
                case ApplicationStates.Initializing:
                    break;
                case ApplicationStates.Leaderboard:
                    break;
                case ApplicationStates.MatchTab:
                    break;
                case ApplicationStates.Menu:
                    break;
                case ApplicationStates.Session:
                    SessionModelsHandler(modelInstance);
                    break;
                case ApplicationStates.SnapshotTab:
                    break;
            }
        }

        private void MenuEventHandler(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                case evolute_duel_GameCreated gameCreated:
                    GameUpdated updated = new GameUpdated().SetData(gameCreated);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {gameCreated.Model.Name} ");
                    EventBus.Publish(updated);
                    break;
                case evolute_duel_GameCanceled gameCanceled:
                    GameUpdated canceled = new GameUpdated().SetData(gameCanceled);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {gameCanceled.Model.Name} ");
                    EventBus.Publish(canceled);
                    break;
                case evolute_duel_GameJoinFailed joinFailed:
                    string localPLayerAddress = DojoGameManager.Instance.LocalAccount.Address.Hex();
                    if (localPLayerAddress != joinFailed.host_player.Hex() &&
                        localPLayerAddress != joinFailed.guest_player.Hex())
                    {
                        return;
                    }
                    ErrorOccured errorOccured = new ErrorOccured().SetData(joinFailed);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {joinFailed.Model.Name} ");
                    EventBus.Publish(errorOccured);
                    break;
            }
        }

        private void SessionEventHandler(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                // session
                case evolute_duel_BoardUpdated boardUpdated:
                    if (!_globalContext.SessionContext.IsSessionBoard(boardUpdated.board_id.Hex()))
                    {
                        return;
                    }

                    BoardUpdated boardUpdate = new BoardUpdated().SetData(boardUpdated);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {boardUpdated.Model.Name} ");
                    EventBus.Publish(boardUpdate);
                    break;
                case evolute_duel_Moved moved:
                    if (!_globalContext.SessionContext.IsPlayerInSession(moved.player.Hex()))
                    {
                        return;
                    }

                    Moved move = new Moved().SetData(moved);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {moved.Model.Name} ");
                    EventBus.Publish(move);
                    break;
                case evolute_duel_Skiped skipped:
                    if (!_globalContext.SessionContext.IsPlayerInSession(skipped.player.Hex()))
                    {
                        return;
                    }

                    Skipped skip = new Skipped().SetData(skipped);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {skipped.Model.Name} ");
                    EventBus.Publish(skip);
                    break;

                case evolute_duel_InvalidMove invalidMove:
                    if (_globalContext.SessionContext.IsPlayerInSession(invalidMove.player.Hex()))
                    {
                        return;
                    }

                    ErrorOccured errorOccured = new ErrorOccured().SetData(invalidMove);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {invalidMove.Model.Name} ");
                    EventBus.Publish(errorOccured);
                    break;
                case evolute_duel_NotYourTurn notYourTurn:
                    if (_globalContext.PlayerProfile.PlayerId != notYourTurn.player_id.Hex())
                    {
                        return;
                    }

                    ErrorOccured notYourTurnError = new ErrorOccured().SetData(notYourTurn);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {notYourTurn.Model.Name} ");
                    EventBus.Publish(notYourTurnError);
                    break;

                case evolute_duel_GameFinished gameFinished:
                    if (!_globalContext.SessionContext.IsSessionBoard(gameFinished.board_id.Hex()))
                    {
                        return;
                    }

                    GameFinished gameFinishedEvent = new GameFinished().SetData(gameFinished);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {gameFinished.Model.Name} ");
                    EventBus.Publish(gameFinishedEvent);
                    break;
                case evolute_duel_GameIsAlreadyFinished gameIsAlreadyFinished:
                    if (!_globalContext.SessionContext.IsSessionBoard(gameIsAlreadyFinished.board_id.Hex()))
                    {
                        return;
                    }

                    GameFinished gameIsAlreadyFinishedEvent = new GameFinished().SetData(gameIsAlreadyFinished);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {gameIsAlreadyFinished.Model.Name} ");
                    EventBus.Publish(gameIsAlreadyFinishedEvent);
                    break;
                case evolute_duel_RoadContestWon roadContestWon:
                    if (!_globalContext.SessionContext.IsSessionBoard(roadContestWon.board_id.Hex()))
                    {
                        return;
                    }
                    Contested contestedRoad = new Contested().SetData(roadContestWon);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {roadContestWon.Model.Name} ");
                    EventBus.Publish(contestedRoad);
                    break;
                case evolute_duel_RoadContestDraw roadContestDraw:
                    if (!_globalContext.SessionContext.IsSessionBoard(roadContestDraw.board_id.Hex()))
                    {
                        return;
                    }
                    Contested contestedRoadDraw = new Contested().SetData(roadContestDraw);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {roadContestDraw.Model.Name} ");
                    EventBus.Publish(contestedRoadDraw);
                    break;
                case evolute_duel_CityContestWon cityContestWon:
                    if (!_globalContext.SessionContext.IsSessionBoard(cityContestWon.board_id.Hex()))
                    {
                        return;
                    }
                    Contested contestedCity = new Contested().SetData(cityContestWon);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {cityContestWon.Model.Name} ");
                    EventBus.Publish(contestedCity);
                    break;
                case evolute_duel_CityContestDraw cityContestDraw:
                    if (!_globalContext.SessionContext.IsSessionBoard(cityContestDraw.board_id.Hex()))
                    {
                        return;
                    }
                    Contested contestedCityDraw = new Contested().SetData(cityContestDraw);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {cityContestDraw.Model.Name} ");
                    EventBus.Publish(contestedCityDraw);
                    break;
                case evolute_duel_GameCanceled canceled:
                    if (!_globalContext.SessionContext.IsPlayerInSession(canceled.host_player.Hex()))
                    {
                        return;
                    }
                    GameCanceled gameCanceled = new GameCanceled().SetData(canceled);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {canceled.Model.Name} ");
                    EventBus.Publish(gameCanceled);
                    break;
                case evolute_duel_PhaseStarted phaseStarted:
                    if (!_globalContext.SessionContext.IsSessionBoard(phaseStarted.board_id.Hex()))
                    {
                        return;
                    }
                    PhaseStarted phaseStartedEvent = new PhaseStarted().SetData(phaseStarted);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {phaseStarted.Model.Name} ");
                    EventBus.Publish(phaseStartedEvent);
                    break;
            }
        }

        public void SessionModelsHandler(ModelInstance modelInstance)
        {
            if (DojoGameManager.Instance.IsTargetModel(modelInstance, nameof(evolute_duel_UnionFind)))
            {
                evolute_duel_UnionFind unionFindModel = modelInstance as evolute_duel_UnionFind;
                if (unionFindModel == null || unionFindModel.board_id == null)
                {
                    CustomLogger.LogError($"[EventHandler] | {unionFindModel} is null");
                    return;
                }
                if (_globalContext.SessionContext.IsSessionBoard(unionFindModel.board_id?.Hex()) == false)
                {
                    CustomLogger.LogEventsAll($"[EventHandler] | {unionFindModel.Model.Name } | Not session board: {unionFindModel.board_id?.Hex()}");
                    return;
                }
                UnionFind unionFind = new UnionFind().SetData(unionFindModel);
                CustomLogger.LogEventsLocal($"[EventHandler] | {unionFindModel.Model.Name }");
                EventBus.Publish(unionFind);
                    
            }

        }


        public void Dispose()
        {
            _worldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnEventMessage);
            _worldManager.synchronizationMaster.OnModelUpdated.RemoveListener(OnModelUpdated);
            IncomingModelsFilter.OnModelPassed.RemoveListener(OnModelUpdated);
        }
    }
}
