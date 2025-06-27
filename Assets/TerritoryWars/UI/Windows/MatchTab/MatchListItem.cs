using System.Collections;
using TerritoryWars.ConnectorLayers.WebSocketLayer;
using TerritoryWars.Tools;
using TerritoryWars.UI.General;
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
        public OnlineStatus OnlineStatus;
        
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
            OnlineStatus.Initialize(HostPlayer);
        }

        
        
        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }
    }
}