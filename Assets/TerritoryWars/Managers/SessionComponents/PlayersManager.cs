using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class PlayersManager: ISessionComponent
    {
        private SessionContext _context;

        public void Initialize(SessionContext context)
        {
            _context = context;
        }
        
        public void SetupPlayers() { }
        public void SpawnPlayers() { }
        public void ShowPlayers() { }

        public void Dispose() { }
    }
}