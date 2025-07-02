using System.Collections;
using TerritoryWars.ConnectorLayers.WebSocketLayer;
using TerritoryWars.DataModels.ClientEvents;
using TerritoryWars.DataModels.WebSocketEvents;
using TerritoryWars.General;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI.General
{
    public class OnlineStatus : MonoBehaviour
    {
        public bool IsOnline => OnlineStatusImage.color == WebSocketClient.Configuration.OnlineStatusColor;
        public Image OnlineStatusImage;

        private string PlayerAddress;
        
        private Coroutine _onlineStatusCoroutine;
        
        public void Initialize(string playerAddress)
        {
            PlayerAddress = playerAddress;
            UpdateOnline();
            EventBus.Subscribe<PingEvent>(OnPingEvent);
        }

        public void InitializeForce(bool status)
        {
            OnlineStatusImage.color = status ? WebSocketClient.Configuration.OnlineStatusColor 
                                             : WebSocketClient.Configuration.OfflineStatusColor;
            EventBus.Publish(new OnlineStatusChanged(){Address = PlayerAddress, IsOnline = status});
        }

        private void OnPingEvent(PingEvent pingEvent)
        {
            if (PlayerAddress == pingEvent.Address)
            {
                UpdateOnline();
            }
        }

        public void SetOnline(bool online)
        {
            if (online)
            {
                UpdateOnline();
            }
            else
            {
                if (_onlineStatusCoroutine != null) 
                {
                    StopCoroutine(_onlineStatusCoroutine);
                    _onlineStatusCoroutine = null;
                }
                OnlineStatusImage.color = WebSocketClient.Configuration.OfflineStatusColor;
                EventBus.Publish(new OnlineStatusChanged(){Address = PlayerAddress, IsOnline = false});
            }
            
        }

        public void UpdateOnline()
        {
            if (_onlineStatusCoroutine != null)
            {
                StopCoroutine(_onlineStatusCoroutine);
                _onlineStatusCoroutine = null;
            }

            _onlineStatusCoroutine = StartCoroutine(OnlineStatusCoroutine());
        }

        private IEnumerator OnlineStatusCoroutine()
        {
            EventBus.Publish(new OnlineStatusChanged(){Address = PlayerAddress, IsOnline = true});
            OnlineStatusImage.color = WebSocketClient.Configuration.OnlineStatusColor;
            yield return new WaitForSeconds(WebSocketClient.Configuration.PingInterval + 1f);
            OnlineStatusImage.color = WebSocketClient.Configuration.OfflineStatusColor;
            EventBus.Publish(new OnlineStatusChanged(){Address = PlayerAddress, IsOnline = false});
        }
        
        private void OnDestroy()
        {
            if (_onlineStatusCoroutine != null)
            {
                StopCoroutine(_onlineStatusCoroutine);
                _onlineStatusCoroutine = null;
            }
            EventBus.Unsubscribe<PingEvent>(OnPingEvent);
        }
    }
}