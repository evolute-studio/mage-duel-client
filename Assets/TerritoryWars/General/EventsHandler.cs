using Dojo;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;

namespace TerritoryWars.General
{
    public class EventsHandler
    {
        private WorldManager _worldManager;
        
        public EventsHandler(WorldManager worldManager)
        {
            _worldManager = worldManager;
            _worldManager.synchronizationMaster.OnEventMessage.AddListener(OnEventMessage);
        }
        
        private void OnEventMessage(ModelInstance modelInstance)
        {
            switch (modelInstance)
            {
                // session
                case evolute_duel_BoardUpdated boardUpdated:
                    BoardUpdated boardUpdate = new BoardUpdated().SetData(boardUpdated);
                    evolute_duel_Board board = _worldManager.EntityModel<evolute_duel_Board>("id", boardUpdated.board_id);
                    Board.AddEdgeTiles(boardUpdate.Tiles, board.initial_edge_state);
                    EventBus.Publish(boardUpdate);
                    break;
                case evolute_duel_Moved moved:
                    Moved move = new Moved().SetData(moved);
                    EventBus.Publish(move);
                    break;
                case evolute_duel_Skiped skiped:
                    Skipped skip = new Skipped().SetData(skiped);
                    EventBus.Publish(skip);
                    break;
                
                case evolute_duel_InvalidMove invalidMove:
                    ErrorOccured errorOccured = new ErrorOccured().SetData(invalidMove);
                    EventBus.Publish(errorOccured);
                    break;
                case evolute_duel_NotYourTurn notYourTurn:
                    ErrorOccured notYourTurnError = new ErrorOccured().SetData(notYourTurn);
                    EventBus.Publish(notYourTurnError);
                    break;
                
                case evolute_duel_GameFinished gameFinished:
                    GameFinished gameFinishedEvent = new GameFinished().SetData(gameFinished);
                    EventBus.Publish(gameFinishedEvent);
                    break;
                case evolute_duel_GameIsAlreadyFinished gameIsAlreadyFinished:
                    GameFinished gameIsAlreadyFinishedEvent = new GameFinished().SetData(gameIsAlreadyFinished);
                    EventBus.Publish(gameIsAlreadyFinishedEvent);
                    break;
                case evolute_duel_RoadContestWon roadContestWon:
                    Contested contestedRoad = new Contested().SetData(roadContestWon);
                    EventBus.Publish(contestedRoad);
                    break;
                case evolute_duel_RoadContestDraw roadContestDraw:
                    Contested contestedRoadDraw = new Contested().SetData(roadContestDraw);
                    EventBus.Publish(contestedRoadDraw);
                    break;
                case evolute_duel_CityContestWon cityContestWon:
                    Contested contestedCity = new Contested().SetData(cityContestWon);
                    EventBus.Publish(contestedCity);
                    break;
                case evolute_duel_CityContestDraw cityContestDraw:
                    Contested contestedCityDraw = new Contested().SetData(cityContestDraw);
                    EventBus.Publish(contestedCityDraw);
                    break;
                case evolute_duel_GameCanceled canceled:
                    GameCanceled gameCanceled = new GameCanceled().SetData(canceled);
                    EventBus.Publish(gameCanceled);
                    break;
            }
        }
    }
}