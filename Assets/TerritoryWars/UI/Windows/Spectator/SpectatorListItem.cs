using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TerritoryWars.UI.Windows.Spectator
{
    public class SpectatorListItem : MonoBehaviour
    {
        public GameObject ListItem;
        public string FirstPlayerName;
        public string SecondPlayerName;
        public uint FirstPlayerScore;
        public uint SecondPlayerScore;
        public string BoardId;
        
        public Button WatchButton;
        public TextMeshProUGUI FirstPlayerNameText;
        public TextMeshProUGUI SecondPlayerNameText;
        public TextMeshProUGUI FirstPlayerScoreText;
        public TextMeshProUGUI SecondPlayerScoreText;
        public TextMeshProUGUI FirstPlayerEvoluteCountText;
        public TextMeshProUGUI SecondPlayerEvoluteCountText;

        public void UpdateItem(string firstPlayerName, string secondPlayerName, string status, uint fistPlayerScore,
            uint secondPlayerScore, uint firstPlayerEvoluteCount, uint secondPlayerEvoluteCount, string boardId, UnityAction onJoin = null)
        {
            FirstPlayerName = firstPlayerName;
            SecondPlayerName = secondPlayerName;
            FirstPlayerScore = fistPlayerScore;
            SecondPlayerScore = secondPlayerScore;
            
            BoardId = boardId;
            
            FirstPlayerNameText.text = FirstPlayerName;
            SecondPlayerNameText.text = SecondPlayerName;
            FirstPlayerScoreText.text = FirstPlayerScore.ToString();
            SecondPlayerScoreText.text = SecondPlayerScore.ToString();
            FirstPlayerEvoluteCountText.text = firstPlayerEvoluteCount + ")";
            SecondPlayerEvoluteCountText.text = secondPlayerEvoluteCount + ")";
     
            WatchButton.onClick.RemoveAllListeners();

            if (status != "In Progress")
            {
                WatchButton.interactable = false;
            }
            else
            {
                WatchButton.interactable = true;
                if (onJoin != null)
                {
                    WatchButton.onClick.AddListener(onJoin);
                }
            }
        }
        
        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }
    }
}