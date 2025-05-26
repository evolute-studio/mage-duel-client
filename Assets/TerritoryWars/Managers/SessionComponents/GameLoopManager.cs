using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;
using TerritoryWars.General;

namespace TerritoryWars.Managers.SessionComponents
{
    public class GameLoopManager: ISessionComponent
    {
        private SessionContext _context;
        private TurnEndData _turnEndData = new TurnEndData();

        public void Initialize(SessionContext context)
        {
            _context = context;
            EventBus.Subscribe<TurnEndData>(OnTurnEnd);
            EventBus.Subscribe<BoardUpdated>((data) => _turnEndData.SetBoardUpdated(ref data));
            EventBus.Subscribe<Moved>((data) => _turnEndData.SetMoved(ref data));
            EventBus.Subscribe<Skipped>((data) => _turnEndData.SetSkipped(ref data));
        }

        public void StartGame() { }
        public void EndGame() { }

        public void OnTurnEnd(TurnEndData turnEndData)
        {
        }

        private void StartTurn() { }

        public void EndTurn() { }

        public void Dispose()
        {
            EventBus.Unsubscribe<TurnEndData>(OnTurnEnd);
            EventBus.Unsubscribe<BoardUpdated>((data) => _turnEndData.SetBoardUpdated(ref data));
            EventBus.Unsubscribe<Moved>((data) => _turnEndData.SetMoved(ref data));
            EventBus.Unsubscribe<Skipped>((data) => _turnEndData.SetSkipped(ref data));
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