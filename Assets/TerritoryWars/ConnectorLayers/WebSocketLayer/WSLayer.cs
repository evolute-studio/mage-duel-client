using System;
using TerritoryWars.DataModels.WebSocketEvents;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.WebSocketLayer
{
    public class WSLayer: MonoBehaviour
    {
        public static WSLayer Instance { get; private set; }
        
        private string _currentSessionChannel = string.Empty;
        
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

        public void SubscribeSessionChannel(string boardId)
        {
            _currentSessionChannel = nameof(WSChannels.Session) + "_" + boardId;
            WebSocketClient.Subscribe(_currentSessionChannel);
        }

        public void UnsubscribeSessionChannel()
        {
            WebSocketClient.Unsubscribe(_currentSessionChannel);
            _currentSessionChannel = string.Empty;
        }

        public void SendMovePreview(MoveSneakPeek moveSneakPeek)
        {
            WebSocketClient.Publish(_currentSessionChannel, JsonUtility.ToJson(moveSneakPeek));
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