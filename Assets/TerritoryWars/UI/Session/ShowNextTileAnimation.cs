using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TerritoryWars.UI.Session
{
    public class ShowNextTileAnimation
    {
        private readonly RectTransform _currentTileRect;
        private readonly RectTransform _nextTileRect;
        private readonly CanvasGroup _nextTileCanvasGroup;
        

        private Vector2Int _leftPosition = new Vector2Int(-215, 0);
        private Vector2Int _rightPosition = new Vector2Int(81, 0);
        private readonly float _leftScale = 0.75f;
        private readonly float _rightScale  = 1;
        private readonly float _leftAlpha = 1f;
        private readonly float _rightAlpha = 0.01f;
        
        private Action _callback;
        private bool _isShown = false;
        

        public ShowNextTileAnimation(RectTransform currentTileRect, RectTransform nextTileRect, CanvasGroup nextTileCanvasGroup)
        {
            _currentTileRect = currentTileRect;
            _nextTileRect = nextTileRect;
            _nextTileCanvasGroup = nextTileCanvasGroup;
            
            Reset();
        }

        public ShowNextTileAnimation Show(Action callback = null)
        {
            ShowNextTile(callback);
            return this;
        }

        public ShowNextTileAnimation Hide(Action callback = null)
        {
            Hide_Start(callback);
            return this;
        }

        private void Reset()
        {
            // current tile (main)
            _currentTileRect.anchoredPosition = new Vector2(-244.2f, 142.9f);
            
            // next tile
            _nextTileRect.anchoredPosition = new Vector2(-184.995f, 142.9f);
            _nextTileRect.localScale = new Vector2(0.75f, 0.75f);
            _nextTileCanvasGroup.alpha = 0;
            
            // layering
            _currentTileRect.SetAsLastSibling();
        }

        private void ShowNextTile(Action callback = null)
        {
            float duration = 0.5f;
            
            //current tile (main)
            Vector2 currentTile_Pos = new Vector2(-338f, 142.9f);

            _currentTileRect.DOAnchorPos(currentTile_Pos, duration).SetEase(Ease.OutBack);
            
            // next tile
            Vector2 nextTile_Pos = new Vector2(-143f, 142.9f);
            float nextTile_Scale = 0.5f;
            
            _nextTileCanvasGroup.alpha = 1;
            _nextTileRect.DOAnchorPos(nextTile_Pos, duration).SetEase(Ease.OutBack);
            _nextTileRect.DOScale(nextTile_Scale, duration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _callback?.Invoke();
                _callback = null;
            });
        }

        private void Hide_Start(Action callback = null)
        {
            float duration = 0.5f;
            Vector2 nextTile_Pos = new Vector2(-87f, 142.9f);
            
            _nextTileRect.DOAnchorPos(nextTile_Pos, duration).SetEase(Ease.InBack).OnComplete(() =>
                {
                    _nextTileRect.SetAsLastSibling();
                    Hide_Finish(callback);
                });
        }

        private void Hide_Finish(Action callback = null)
        {
            float allDuration = 0.5f;
            
            // current tile (main)
            Vector2 currentTile_Pos = new Vector2(-244.2f, 142.9f);
            
            _currentTileRect.DOAnchorPos(currentTile_Pos, allDuration).SetEase(Ease.InBack);
            
            // next tile
            Vector2 nextTile_Pos = new Vector2(-184.995f, 142.9f);
            float nextTile_Scale = 0.75f;
            float nextTile_Alpha = 0.01f;
            float opacityDuration = 0.3f;
            _nextTileCanvasGroup.DOFade(nextTile_Alpha, opacityDuration).SetEase(Ease.InBack);
            _nextTileRect.DOAnchorPos(nextTile_Pos, allDuration).SetEase(Ease.InBack);
            _nextTileRect.DOScale(nextTile_Scale, allDuration).SetEase(Ease.InBack).OnComplete(() =>
            {
                _currentTileRect.SetAsLastSibling();
                _callback?.Invoke();
                _callback = null;
            });
        }


        public ShowNextTileAnimation OnComplete(Action onComplete)
        {
            _callback = onComplete;
            return this;
        }
    }
}