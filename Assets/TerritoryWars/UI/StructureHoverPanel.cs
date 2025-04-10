using TerritoryWars.General;
using TMPro;
using UnityEngine;

namespace TerritoryWars.UI
{
    public class StructureHoverPanel : MonoBehaviour
    {
        public TextMeshProUGUI BlueScoreText;
        public TextMeshProUGUI RedScoreText;
        
        public void SetScores(ushort blueScore, ushort redScore)
        {
            if (SessionManager.Instance.IsLocalPlayerHost)
            {
                BlueScoreText.text = blueScore.ToString();
                RedScoreText.text = redScore.ToString();
            }
            else
            {
                BlueScoreText.text = redScore.ToString();
                RedScoreText.text = blueScore.ToString();
            }
            
        }
    }
}