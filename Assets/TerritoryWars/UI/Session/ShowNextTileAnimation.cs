using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace TerritoryWars.UI.Session
{
    public class ShowNextTileAnimation
    {
        private readonly CanvasGroup _backgroundCanvasGroup;
        private readonly RectTransform _backgroundCircleRect;

        private Vector2Int _leftPosition = new Vector2Int(-215, 0);
        private Vector2Int _rightPosition = new Vector2Int(81, 0);
        private readonly float _leftScale = 0.75f;
        private readonly float _rightScale  = 1;
        private readonly float _leftAlpha = 1f;
        private readonly float _rightAlpha = 0.01f;
        
        private Action _callback;
        private bool _isShown = false;
        

        public ShowNextTileAnimation(CanvasGroup backgroundCanvasGroup, RectTransform backgroundCircleRect)
        {
            _backgroundCanvasGroup = backgroundCanvasGroup;
            _backgroundCircleRect = backgroundCircleRect;
        }

        public ShowNextTileAnimation Show()
        {
            
            _backgroundCanvasGroup.alpha = 0;
            _backgroundCircleRect.localScale = new Vector3(_rightScale, _rightScale, 1);
            _backgroundCircleRect.anchoredPosition = new Vector2(_rightPosition.x, _rightPosition.y);
            
            
            _backgroundCircleRect.DOAnchorPosX(_leftPosition.x, 0.5f).SetEase(Ease.OutBack);
            _backgroundCircleRect.DOScale(_leftScale, 0.5f).SetEase(Ease.OutBack);
            _backgroundCanvasGroup.DOFade(_leftAlpha, 0.5f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _callback?.Invoke();
                _callback = null;
                _isShown = true;
            });
            
            
            
            return this;
        }

        public ShowNextTileAnimation Hide()
        {
            // _backgroundCanvasGroup.alpha = 1;
            // _backgroundCircleRect.localScale = new Vector3(_leftScale, _leftScale, 1);
            // _backgroundCircleRect.anchoredPosition = new Vector2(_leftPosition.x, _leftPosition.y);
            
            _backgroundCircleRect.DOAnchorPosX(_rightPosition.x, 0.5f).SetEase(Ease.InBack);
            _backgroundCanvasGroup.DOFade(_rightAlpha, 0.3f).SetEase(Ease.InBack);
            _backgroundCircleRect.DOScale(_rightScale, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
            {
                _backgroundCanvasGroup.alpha = 0;
                _callback?.Invoke();
                _callback = null;
                _isShown = false;
            });
            
            return this;
        }

        public ShowNextTileAnimation OnComplete(Action onComplete)
        {
            _callback = onComplete;
            return this;
        }
    }
}