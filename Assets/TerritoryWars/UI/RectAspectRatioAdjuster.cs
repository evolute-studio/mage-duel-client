using UnityEngine;

namespace TerritoryWars.UI
{
    public class RectAspectRatioAdjuster : MonoBehaviour
    {
        public Vector2 TargetScreenSize = new Vector2(1920, 1080);
        public float TargetAspectRatio => TargetScreenSize.x / TargetScreenSize.y;
        
        private Vector2 _currentScreenSize = new Vector2(1920, 1080);
        public float CurrentAspectRatio => _currentScreenSize.x / _currentScreenSize.y;
        
        private RectTransform _rectTransform;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            
        }

        public void Update()
        {
            if (!Mathf.Approximately(_currentScreenSize.x, Screen.width) || !Mathf.Approximately(_currentScreenSize.y, Screen.height))
            {
                AdjustAspectRatio();
            }
        }

        public void AdjustAspectRatio()
        {
            _currentScreenSize.x = Screen.width;
            _currentScreenSize.y = Screen.height;
            float currentAspectRatio = CurrentAspectRatio;
            Vector3 scale = new Vector3(TargetAspectRatio / currentAspectRatio, 1, 1);
            _rectTransform.localScale = scale;
        }
    }
}