using System;
using System.Collections;
using System.Collections.Generic;
using TerritoryWars.DataModels.WebSocketEvents;
using TerritoryWars.Dojo;
using TerritoryWars.Tools;
using UnityEngine;

namespace TerritoryWars.ConnectorLayers.WebSocketLayer
{
    public class Pinger
    {
        private float _pingInterval => WebSocketClient.Configuration.PingInterval;

        public void Initialize()
        {
            Coroutines.StartRoutine(PingCoroutine());
        }

        private IEnumerator PingCoroutine()
        {
            while (true)
            {
                PingEvent();
                yield return new WaitForSeconds(_pingInterval);
            }
        }
        
        
        private void PingEvent()
        {
            string accountAddress = DojoGameManager.Instance?.LocalAccount?.Address?.Hex();
            if (string.IsNullOrEmpty(accountAddress))
            {
                return;
            }

            PingEvent pingEvent = new PingEvent()
            {
                Address = accountAddress,
                Timestamp = (ulong)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds,
            };
            string payload = JsonUtility.ToJson(pingEvent);
            WebSocketClient.Publish(nameof(WSChannels.Ping), payload);
        }
    }

    
}