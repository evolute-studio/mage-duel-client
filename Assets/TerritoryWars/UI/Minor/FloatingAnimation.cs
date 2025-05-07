using DG.Tweening;
using UnityEngine;

namespace TerritoryWars.UI.Minor
{
    public class FloatingAnimation : MonoBehaviour
    {
        private RectTransform rectTransform;
        
        public float floatSpeed = 1f;
        public float floatHeight = 0.5f;
        
        private Vector3 _initialPosition;

        public void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError("FloatingAnimation script requires a RectTransform component.");
                return;
            }
            _initialPosition = rectTransform.anchoredPosition;
            DOTweenAnimation();
        }

        private void DOTweenAnimation()
        {
            rectTransform.DOLocalMoveY(_initialPosition.y + floatHeight, floatSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
}