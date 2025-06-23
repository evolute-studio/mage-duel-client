using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
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
            ShowNextTile();
            return this;
        }

        public ShowNextTileAnimation Hide(Action callback = null)
        {
            if (IsDefaultState())
            {
                _callback = callback;
                _callback?.Invoke();
                _callback = null;
                return this;
            }
            Hide_Start();
            return this;
        }

        private void Reset()
        {
            // current tile (main)
            _currentTileRect.anchoredPosition = new Vector2(-244.2f, 142.9f);
            
            // next tile
            _nextTileRect.anchoredPosition = new Vector2(-184.2f, 142.9f);
        }
        
        private bool IsDefaultState()
        {
            return _currentTileRect.anchoredPosition == new Vector2(-244.2f, 142.9f);
        }

        private void ShowNextTile()
        {
            float duration = 0.5f;
            
            //current tile (main)
            Vector2 currentTile_Pos = new Vector2(-393.1f, 142.9f);

            _currentTileRect.DOAnchorPos(currentTile_Pos, duration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _callback?.Invoke();
                _callback = null;
            });
        }

        private void Hide_Start()
        {
            float duration = 0.5f;
            Vector2 currentTile_Pos = new Vector2(-393.1f, -198.7f);
            
            _currentTileRect.DOAnchorPos(currentTile_Pos, duration).SetEase(Ease.InBack).OnComplete(() =>
                {
                    Reset();
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