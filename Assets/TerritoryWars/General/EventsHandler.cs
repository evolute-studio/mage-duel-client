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
            CustomLogger.LogImportant("EventsHandler initialized");
            _worldManager = worldManager;
            _worldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
            //_worldManager.synchronizationMaster.OnModelUpdated.AddListener(OnModelUpdated);
            IncomingModelsFilter.OnModelPassed.AddListener(OnModelUpdated);
        }

        private void OnEventMessage(ModelInstance modelInstance)
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

        private void SessionEventHandler(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                // session
                case evolute_duel_BoardUpdated boardUpdated:
                    if (!_globalContext.SessionContext.IsSessionBoard(boardUpdated.board_id.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(boardUpdated)}");
                        return;
                    }

                    BoardUpdated boardUpdate = new BoardUpdated().SetData(boardUpdated);
                    CustomLogger.LogObject(boardUpdate, "BoardUpdated from Event");
                    // evolute_duel_Board board = _worldManager.EntityModel<evolute_duel_Board>("id", boardUpdated.board_id);
                    // Board.AddEdgeTiles(boardUpdate.Tiles, board.initial_edge_state);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(boardUpdated)}");
                    EventBus.Publish(boardUpdate);
                    break;
                case evolute_duel_Moved moved:
                    if (!_globalContext.SessionContext.IsPlayerInSession(moved.player.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(moved)}");
                        return;
                    }

                    Moved move = new Moved().SetData(moved);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(moved)}");
                    EventBus.Publish(move);
                    break;
                case evolute_duel_Skiped skipped:
                    if (!_globalContext.SessionContext.IsPlayerInSession(skipped.player.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(skipped)}");
                        return;
                    }

                    Skipped skip = new Skipped().SetData(skipped);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(skipped)}");
                    EventBus.Publish(skip);
                    break;

                case evolute_duel_InvalidMove invalidMove:
                    if (_globalContext.SessionContext.IsPlayerInSession(invalidMove.player.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(invalidMove)}");
                        return;
                    }

                    ErrorOccured errorOccured = new ErrorOccured().SetData(invalidMove);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(invalidMove)}");
                    EventBus.Publish(errorOccured);
                    break;
                case evolute_duel_NotYourTurn notYourTurn:
                    if (_globalContext.PlayerProfile.PlayerId != notYourTurn.player_id.Hex())
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(notYourTurn)}");
                        return;
                    }

                    ErrorOccured notYourTurnError = new ErrorOccured().SetData(notYourTurn);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(notYourTurn)}");
                    EventBus.Publish(notYourTurnError);
                    break;

                case evolute_duel_GameFinished gameFinished:
                    if (!_globalContext.SessionContext.IsSessionBoard(gameFinished.board_id.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(gameFinished)}");
                        return;
                    }

                    GameFinished gameFinishedEvent = new GameFinished().SetData(gameFinished);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(gameFinished)}");
                    EventBus.Publish(gameFinishedEvent);
                    break;
                case evolute_duel_GameIsAlreadyFinished gameIsAlreadyFinished:
                    if (!_globalContext.SessionContext.IsSessionBoard(gameIsAlreadyFinished.board_id.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(gameIsAlreadyFinished)}");
                        return;
                    }

                    GameFinished gameIsAlreadyFinishedEvent = new GameFinished().SetData(gameIsAlreadyFinished);

                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(gameIsAlreadyFinished)}");
                    EventBus.Publish(gameIsAlreadyFinishedEvent);
                    break;
                case evolute_duel_RoadContestWon roadContestWon:
                    if (!_globalContext.SessionContext.IsSessionBoard(roadContestWon.board_id.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(roadContestWon)}");
                        return;
                    }
                    Contested contestedRoad = new Contested().SetData(roadContestWon);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(roadContestWon)}");
                    EventBus.Publish(contestedRoad);
                    break;
                case evolute_duel_RoadContestDraw roadContestDraw:
                    if (!_globalContext.SessionContext.IsSessionBoard(roadContestDraw.board_id.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(roadContestDraw)}");
                        return;
                    }
                    Contested contestedRoadDraw = new Contested().SetData(roadContestDraw);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(roadContestDraw)}");
                    EventBus.Publish(contestedRoadDraw);
                    break;
                case evolute_duel_CityContestWon cityContestWon:
                    if (!_globalContext.SessionContext.IsSessionBoard(cityContestWon.board_id.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(cityContestWon)}");
                        return;
                    }
                    Contested contestedCity = new Contested().SetData(cityContestWon);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(cityContestWon)}");
                    EventBus.Publish(contestedCity);
                    break;
                case evolute_duel_CityContestDraw cityContestDraw:
                    if (!_globalContext.SessionContext.IsSessionBoard(cityContestDraw.board_id.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(cityContestDraw)}");
                        return;
                    }
                    Contested contestedCityDraw = new Contested().SetData(cityContestDraw);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(cityContestDraw)}");
                    EventBus.Publish(contestedCityDraw);
                    break;
                case evolute_duel_GameCanceled canceled:
                    if (!_globalContext.SessionContext.IsPlayerInSession(canceled.host_player.Hex()))
                    {
                        CustomLogger.LogEventsAll($"[EventHandler] | {nameof(canceled)}");
                        return;
                    }
                    GameCanceled gameCanceled = new GameCanceled().SetData(canceled);
                    CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(canceled)}");
                    EventBus.Publish(gameCanceled);
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
                    CustomLogger.LogError($"[EventHandler] | {nameof(unionFindModel)} is null");
                    return;
                }
                if (_globalContext.SessionContext.IsSessionBoard(unionFindModel.board_id?.Hex()) == false)
                {
                    CustomLogger.LogEventsAll($"[EventHandler] | {nameof(unionFindModel)} | Not session board: {unionFindModel.board_id?.Hex()}");
                    return;
                }
                UnionFind unionFind = new UnionFind().SetData(unionFindModel);
                CustomLogger.LogEventsLocal($"[EventHandler] | {nameof(unionFindModel)}");
                EventBus.Publish(unionFind);
                    
            }

        }


        public void Dispose()
        {
            _worldManager.synchronizationMaster.OnEventMessage.RemoveListener(OnEventMessage);
            _worldManager.synchronizationMaster.OnModelUpdated.RemoveListener(OnModelUpdated);
            IncomingModelsFilter.OnModelPassed.RemoveListener(OnModelUpdated);
            CustomLogger.LogImportant("EventsHandler disposed");
        }
    }
}
