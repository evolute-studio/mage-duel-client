using DG.Tweening;
using UnityEngine;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.UI.Windows.Leaderboard
{
    public class LeaderboardListItem : MonoBehaviour
    {
        private string _playerName;
        private int _evoluteCount;
        private string _address;
        
        [Header("References")]
        public TextMeshProUGUI PlayerNameText;
        public TextMeshProUGUI EvoluteCountText;
        public TextMeshProUGUI AddressText;
        public TextMeshProUGUI PlaceText;
        public GameObject LeaderPlaceGO;
        public Image LeaderPlaceImage;
        public Button CopyButton;
        public GameObject CopyDescribeGO;

        [Header("Data")] 
        public Sprite[] LeadersImages;
        
        private bool isAnimationPlaying = false;

        public void UpdateItem(string name, int evoluteCount, string address)
        {
            _playerName = name;
            _evoluteCount = evoluteCount;
            _address = address;
            
            PlayerNameText.text = name;
            EvoluteCountText.text = " x " + evoluteCount.ToString();
            AddressText.text = address;
        }

        public void SetLeaderPlace(int place, uint leaderPlace = 3)
        {
            if (place <= leaderPlace)
            {
                PlaceText.text = place.ToString();
                LeaderPlaceImage.sprite = LeadersImages[place - 1];
                LeaderPlaceGO.SetActive(true);
            }
            else
            {
                PlaceText.text = place.ToString();
                LeaderPlaceImage.gameObject.SetActive(false);
                LeaderPlaceGO.SetActive(true);
            }
        }

        public void CopyAddress()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            JSBridge.CopyValue(Address);
#else
            GUIUtility.systemCopyBuffer = _address;
#endif

            if (isAnimationPlaying) return;

            isAnimationPlaying = true;
            CopyDescribeGO.transform.position = new Vector3(CopyButton.gameObject.transform.position.x,
                CopyButton.gameObject.transform.position.y + 0.3f, CopyButton.gameObject.transform.position.z);

            CopyDescribeGO.SetActive(true);
            CopyDescribeGO.GetComponent<Transform>().DOMoveY(CopyButton.gameObject.transform.position.y + 1,
                0.5f).OnComplete(() =>
            {
                CopyDescribeGO.SetActive(false);
                isAnimationPlaying = false;
            });
        }
    }
}

