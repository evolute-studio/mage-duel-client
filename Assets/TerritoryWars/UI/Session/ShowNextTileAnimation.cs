using System;
using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.Managers.SessionComponents;
using TerritoryWars.Tile;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace TerritoryWars.UI.Session
{
    public class ShowNextTileAnimation
    {
        private readonly ShowNextTileAnimationContext _context;
        

        private Vector2Int _leftPosition = new Vector2Int(-215, 0);
        private Vector2Int _rightPosition = new Vector2Int(81, 0);
        private readonly float _leftScale = 0.75f;
        private readonly float _rightScale  = 1;
        private readonly float _leftAlpha = 1f;
        private readonly float _rightAlpha = 0.01f;
        
        private Action _callback;
        private bool _isShown = false;
        

        public ShowNextTileAnimation(ShowNextTileAnimationContext context)
        {
            _context = context;
            
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

        public void Reset()
        {
            // current tile (main)
            _context.CurrentTileRect.anchoredPosition = new Vector2(-244.2f, 142.9f);
            
            // next tile
            _context.NextTileRect.anchoredPosition = new Vector2(-184.2f, 142.9f);
            ActivateBackground(true);
        }

        public void ActivateBackground(bool isCurrent)
        {
            _context.CurrentTileImage.sprite = isCurrent ? _context.ActiveBackgroundSprite : _context.InactiveBackgroundSprite;
            _context.NextTileImage.sprite = isCurrent ? _context.InactiveBackgroundSprite : _context.ActiveBackgroundSprite;
        }
        
        private bool IsDefaultState()
        {
            return _context.CurrentTileRect.anchoredPosition == new Vector2(-244.2f, 142.9f);
        }

        public void DropCurrentTile(Action callback = null)
        {
            TileData tileData = SessionManager.Instance.ManagerContext.GameLoopManager.GetCurrentTile(); 
            GameUI.Instance.TilePreviewUINext.UpdatePreview(tileData);
            GameUI.Instance.ShowNextTileAnimation.ActivateBackground(true);
            
            float duration = 0.5f;
            Vector2 currentTile_Pos = new Vector2(_context.CurrentTileRect.anchoredPosition.x, -198.7f);
            
            _context.CurrentTileRect.DOAnchorPos(currentTile_Pos, duration).SetEase(Ease.InBack).OnComplete(() =>
            {
                Reset();
                GameUI.Instance.TilePreview.UpdatePreview(tileData);
                callback?.Invoke();
            });
        }

        private void ShowNextTile()
        {
            ActivateBackground(true);
            
            float duration = 0.5f;
            
            //current tile (main)
            Vector2 currentTile_Pos = new Vector2(-393.1f, 142.9f);

            _context.CurrentTileRect.DOAnchorPos(currentTile_Pos, duration).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _callback?.Invoke();
                _callback = null;
            });
        }

        private void Hide_Start()
        {
            float duration = 0.5f;
            Vector2 currentTile_Pos = new Vector2(-393.1f, -198.7f);
            
            _context.CurrentTileRect.DOAnchorPos(currentTile_Pos, duration).SetEase(Ease.InBack).OnComplete(() =>
                {
                    Reset();
                    _callback?.Invoke();
                    _callback = null;
                    _context.CurrentTileImage.sprite = _context.ActiveBackgroundSprite;
                });
        }


        public ShowNextTileAnimation OnComplete(Action onComplete)
        {
            _callback = onComplete;
            return this;
        }
    }
    
    [Serializable]
    public struct ShowNextTileAnimationContext
    {
        public Image CurrentTileImage;
        public Image NextTileImage;
        public RectTransform CurrentTileRect;
        public RectTransform NextTileRect;

        public Sprite ActiveBackgroundSprite;
        public Sprite InactiveBackgroundSprite;
    }
}