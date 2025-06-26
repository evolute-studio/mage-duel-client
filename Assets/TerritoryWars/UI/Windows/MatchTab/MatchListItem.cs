using System.Collections;
using TerritoryWars.ConnectorLayers.WebSocketLayer;
using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TerritoryWars.UI.Windows.MatchTab
{
    public class MatchListItem : MonoBehaviour
    {
        public GameObject ListItem;
        public string PlayerName;
        public uint EvoluteCount;
        public string HostPlayer;

        public TextMeshProUGUI PlayerNameText;
        public TextMeshProUGUI AddressText;
        //private TextMeshProUGUI _gameIdText;
        public TextMeshProUGUI EvoluteCountText;
        public TextMeshProUGUI MoveNumberText;
        public Button PlayButton;
        public Image OnlineStatusImage;
        
        private Coroutine _onlineStatusCoroutine;
        
        public void UpdateItem(string playerName, uint evoluteBalance, string status, string hostPlayer, int moveNumber = 0, UnityAction onJoin = null)
        {
            PlayerName = playerName;
            EvoluteCount = evoluteBalance;
            HostPlayer = hostPlayer;

            PlayerNameText.text = PlayerName;
            AddressText.text = hostPlayer;
            EvoluteCountText.text = " x " + EvoluteCount.ToString();
            MoveNumberText.gameObject.SetActive(moveNumber > 0);
            MoveNumberText.text = "Moves: " + moveNumber;
            //_gameIdText.text = GameId;

            PlayButton.onClick.RemoveAllListeners();
            
            if (status != "Created")
            {
                PlayButton.interactable = false;
            }
            else
            {
                
                PlayButton.interactable = true;
                if (onJoin != null)
                {
                    
                    PlayButton.onClick.AddListener(onJoin);
                }
            }
            UpdateOnline();
        }

        public void UpdateOnline()
        {
            Debug.Log("Updating online status for: " + HostPlayer);
            if (_onlineStatusCoroutine != null)
            {
                StopCoroutine(_onlineStatusCoroutine);
                _onlineStatusCoroutine = null;
            }

            _onlineStatusCoroutine = StartCoroutine(OnlineStatusCoroutine());
        }

        private IEnumerator OnlineStatusCoroutine()
        {
            Debug.Log("Starting online status coroutine for: " + HostPlayer);
            OnlineStatusImage.color = WebSocketClient.Configuration.OnlineStatusColor;
            yield return new WaitForSeconds(WebSocketClient.Configuration.PingInterval + 2f);
            Debug.Log("Checking online status for: " + HostPlayer);
            OnlineStatusImage.color = WebSocketClient.Configuration.OfflineStatusColor;
        }
        
        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }
    }
}