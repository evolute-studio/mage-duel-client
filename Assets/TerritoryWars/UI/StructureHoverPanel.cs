using TerritoryWars.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class StructureHoverPanel : MonoBehaviour
    {
        public Image Background;
        public TextMeshProUGUI BlueScoreText;
        public TextMeshProUGUI RedScoreText;
        public Sprite[] ScoreFlags;
        
        public void SetScores(ushort blueScore, ushort redScore)
        {
            if (SessionManagerOld.Instance.IsLocalPlayerHost)
            {
                BlueScoreText.text = blueScore.ToString();
                RedScoreText.text = redScore.ToString();
            }
            else
            {
                BlueScoreText.text = redScore.ToString();
                RedScoreText.text = blueScore.ToString();
            }
            SetFlags(blueScore, redScore);
        }
        
        public void SetFlags(ushort blueScore, ushort redScore)
        {
            if (SessionManagerOld.Instance.IsLocalPlayerHost)
            {
                if(blueScore == redScore)
                    Background.sprite = ScoreFlags[0];
                else if (blueScore > redScore)
                    Background.sprite = ScoreFlags[1];
                else
                    Background.sprite = ScoreFlags[2];
            }
            else
            {
                if(blueScore == redScore)
                    Background.sprite = ScoreFlags[0];
                else if (blueScore < redScore)
                    Background.sprite = ScoreFlags[1];
                else
                    Background.sprite = ScoreFlags[2];
            }
        }
    }
}