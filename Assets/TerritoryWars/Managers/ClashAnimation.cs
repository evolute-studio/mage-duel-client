using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerritoryWars
{
    public class ClashAnimation : MonoBehaviour
    {
        public SpriteAnimator SwordsAnimator;
        public SpriteAnimator FlagsAnimator;
        public SpriteAnimator ContestSpriteAnimator;
        public TextMeshPro SinglePointsText;
        public TextMeshPro[] DoublePointsText;
        public SpriteRenderer BackgroundCircle;
        public SpriteRenderer ContestSprite;
        
        private Queue<Action> _animationQueue = new Queue<Action>();
        private List<List<Sprite>> FlagsAnimations;
        
        public List<Sprite> SwordsAnimations;
        public List<Sprite> FirstPlayerFlagsAnimations;
        public List<Sprite> SecondPlayerFlagsAnimations;
        public List<Sprite> MixedFlagsAnimations;
        
        public Color[] PlayerColors;
        
        [SerializeField] private ContestedSprites[] _roadContestSprites; 
        [SerializeField] private ContestedSprites[] _cityContestSprites;

        private bool _isRoadContest;
        private bool _isCityContest;
        
        
        
        // first action dependencies
        
        // second action dependencies
        private int WinPlayerId;
        
        // third action dependencies
        private Action Recoloring;
        
        // fourth action dependencies
        private ushort[] Points;
        
        public void Initialize(Vector3 position, int winPlayerId, ushort[] points, Action recoloring, bool isRoadContest = false, bool isCityContest = false)
        {
            
            transform.position = position;
            WinPlayerId = SetLocalPlayerData.GetLocalIndex(winPlayerId);
            Points = points;
            Recoloring = recoloring;
            _isRoadContest = isRoadContest;
            _isCityContest = isCityContest;
            
            FlagsAnimations = new List<List<Sprite>>()
            {
                FirstPlayerFlagsAnimations,
                SecondPlayerFlagsAnimations,
                MixedFlagsAnimations
            };
            
            _animationQueue.Enqueue(FirstAction);
            _animationQueue.Enqueue(SecondAction);
            _animationQueue.Enqueue(ThirdAction);
            _animationQueue.Enqueue(FourthAction);
            _animationQueue.Enqueue(FifthAction);
            
            
            NextAction();
        }
        
        private void FirstAction()
        {
            SwordsAnimator.Play(SwordsAnimations.ToArray());
            // SwordsAnimator.OnAnimationEnd = NextAction;
            NextAction();
        }
        
        private void SecondAction()
        {
            if (Points[0] == Points[1])
            {
                WinPlayerId = 2;
            }
            // BackgroundCircle.color = PlayerColors[WinPlayerId];
            // FlagsAnimator.Play(FlagsAnimations[WinPlayerId].ToArray());
            // FlagsAnimator.OnAnimationEnd = NextAction;
            ContestSpriteAnimator.OnAnimationEnd = NextAction;

            if (_isRoadContest)
            {
                ContestSprite.color = new Color(1, 1, 1, 0);
                ContestSpriteAnimator.Play(_roadContestSprites[WinPlayerId].Sprites);
                ContestSprite.DOFade(1f, 0.2f);
            }
            else if (_isCityContest)
            {
                ContestSprite.color = new Color(1, 1, 1, 0);
                ContestSpriteAnimator.Play(_cityContestSprites[WinPlayerId].Sprites);
                ContestSprite.DOFade(1f, 0.2f);
            }
            else
            {
                
            }
            
            // BackgroundCircle.gameObject.SetActive(true);
            if (WinPlayerId == 2)
            {
                SinglePointsText.gameObject.SetActive(false);
                
                DoublePointsText[0].color = new Color(1, 1, 1, 0);
                DoublePointsText[1].color = new Color(1, 1, 1, 0);
                
                DoublePointsText[0].gameObject.SetActive(true);
                DoublePointsText[1].gameObject.SetActive(true);
                
                DoublePointsText[0].text = Points[0].ToString();
                DoublePointsText[1].text = Points[1].ToString();
                
                
                Sequence sequence = DOTween.Sequence();
                sequence.Append(DoublePointsText[0].DOFade(1, 0.2f));
                sequence.Join(DoublePointsText[1].DOFade(1, 0.2f));
            }
            else
            {
                SinglePointsText.gameObject.SetActive(true);
                SinglePointsText.text = (Points[0] + Points[1]).ToString();

                SinglePointsText.color = new Color(1, 1, 1, 0);
                // BackgroundCircle.color = new Color(PlayerColors[WinPlayerId].r, PlayerColors[WinPlayerId].g, PlayerColors[WinPlayerId].b, 0);

                Sequence sequence = DOTween.Sequence();
                sequence.Append(SinglePointsText.DOFade(1, 0.2f));
                // sequence.Join(BackgroundCircle.DOFade(1, 0.2f));
            }
        }
        
        private void ThirdAction()
        {
            Recoloring.Invoke();
            NextAction();
        }
        
        private void FourthAction()
        {
            Invoke(nameof(NextAction), 1f);
        }

        private void FifthAction()
        {
            // List<Sprite> mirrorAnimation = FlagsAnimations[WinPlayerId].ToList();
            // mirrorAnimation.Reverse();
            
            // FlagsAnimator.Play(mirrorAnimation.ToArray());
            
            Sequence sequence = DOTween.Sequence();
            sequence.AppendInterval(0.2f);
            // sequence.Append(FlagsAnimator.GetComponent<SpriteRenderer>().DOFade(0, 0.2f));
            sequence.Join(SwordsAnimator.GetComponent<SpriteRenderer>().DOFade(0, 0.2f));
            sequence.Join(ContestSprite.DOFade(0, 0.2f));
            sequence.Join(SinglePointsText.DOFade(0, 0.2f));
            sequence.Join(DoublePointsText[0].DOFade(0, 0.2f));
            sequence.Join(DoublePointsText[1].DOFade(0, 0.2f));
            // sequence.Join(BackgroundCircle.DOFade(0, 0.2f));
            sequence.AppendCallback(NextAction);
        }

        private void NextAction()
        {
            if (_animationQueue.Count > 0)
            {
                _animationQueue.Dequeue().Invoke();
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }
    
    [Serializable]
    public class ContestedSprites
    {
        public Sprite[] Sprites;
    }
}