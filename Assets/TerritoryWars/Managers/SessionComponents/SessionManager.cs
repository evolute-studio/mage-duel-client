using System;
using System.Collections.Generic;
using UnityEngine;

namespace TerritoryWars.Managers.SessionComponents
{
    public class SessionManager : MonoBehaviour
    {
        private SessionContext _context;
        private List<ISessionComponent> _components;

        private void Start() => Initialize();

        private void Initialize()
        {
            _context = new SessionContext();
            _components = new List<ISessionComponent>();

            var playersManager = new PlayersManager();
            var gameLoopManager = new GameLoopManager();

            _context.PlayersManager = playersManager;
            _context.GameLoopManager = gameLoopManager;

            _components.Add(playersManager);
            _components.Add(gameLoopManager);

            foreach (var component in _components)
                component.Initialize(_context);
        }
        
        // SyncEverythingForGame will be called in another script
        public void SetupData() { }

        private void OnDestroy()
        {
            foreach (var component in _components)
                component.Dispose();
        }
    }
}