using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dojo;
using Dojo.Starknet;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.Events;
using TerritoryWars.Dojo;
using TerritoryWars.ExternalConnections;
using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.Dojo
{
    public class DojoLayer : MonoBehaviour
    {
        public static DojoLayer Instance { get; private set; }
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public WorldManager WorldManager;
        public CustomSynchronizationMaster SynchronizationMaster;
        
        public EventsHandler EventsHandler;
        public string LocalPlayerId => DojoGameManager.Instance.LocalAccount.Address.Hex();

        public void Start()
        {
            EventsHandler = new EventsHandler(WorldManager);
        }

        

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            if (EventsHandler != null)
            {
                EventsHandler.Dispose();
                EventsHandler = null;
            }
        }
    }
}
