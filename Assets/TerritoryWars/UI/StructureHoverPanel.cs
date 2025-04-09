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
            BlueScoreText.text = blueScore.ToString();
            RedScoreText.text = redScore.ToString();
        }
    }
}