using System;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.WebSocketLayer
{
    public class WSLayer: MonoBehaviour
    {
        public static WSLayer Instance { get; private set; }
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

        private Pinger _pinger = new Pinger();

        public void Start()
        {
            WebSocketClient.Initialize();
            _pinger.Initialize();
        }

        public void Update()
        {
            WebSocketClient.Update();
        }

        public void OnDestroy()
        {
            WebSocketClient.Dispose();
        }

        public void OnApplicationQuit()
        {
            WebSocketClient.Dispose();
        }
    }
}