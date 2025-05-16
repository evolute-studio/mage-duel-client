using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.UI.Menu
{
    public class MenuBackground : MonoBehaviour
    {
        [SerializeField] private Image DefaultImage;
        [SerializeField] private Image DarkenImage;
        
        [SerializeField] private Sprite DefaultBackgroundSprite;
        [SerializeField] private Sprite DarkenBackgroundSprite;
        
        [SerializeField] private float TransitionTime = 0.5f;
        [SerializeField] private float AlphaStart = 0.5f;
        [SerializeField] private float AlphaTarget = 0.5f;
        
        [SerializeField] private Transform WindsParent;
        private List<SpriteAnimator> _winds = new List<SpriteAnimator>();
        private int _windIndex = 0;
        private RectTransform _rectTransform;


        private float _targetRatio = 1.777777778f;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            for(int i = 0; i < WindsParent.childCount; i++)
            {
                var wind = WindsParent.GetChild(i).GetComponent<SpriteAnimator>();
                if (wind != null)
                {
                    _winds.Add(wind);
                }
            }
            InvokeRepeating(nameof(StartWinds), 0, 3f);
        }

        public void Update()
        {
            float ratio = Screen.width / (float)Screen.height;
            float scale = ratio > _targetRatio ? ratio / _targetRatio : 1;
            _rectTransform.localScale = new Vector3(scale, scale, 1);
        }

        private void StartWinds()
        {
            _windIndex = (_windIndex + 1) % _winds.Count;
            for (int i = 0; i < _winds.Count; i++)
            {
                _winds[i].gameObject.SetActive(i == _windIndex);
                if (i == _windIndex)
                {
                    _winds[i].Play();
                }
                else
                {
                    _winds[i].Stop();
                }
            }
        }
        
        public void SetBackground(bool darken)
        {
            if (darken)
            {
                DarkenImage.DOFade(AlphaTarget, TransitionTime);
            }
            else
            {
                DarkenImage.DOFade(AlphaStart, TransitionTime);
            }
        }
    }
}