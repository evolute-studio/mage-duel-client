using System.Collections;
using System.Linq;
using DG.Tweening;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace TerritoryWars.UI
{
    public class SessionTimerUI : MonoBehaviour
    {
        [Header("Turn text info")]
        public TextMeshProUGUI TurnText;
        public string LocalPlayerTurnText = "Your turn now";
        public string OpponentPlayerTurnText = $"Waiting for opponent's turn";
        public string PassingTurnText = $"Passing the turn";
        
        [Header("Hourglass")]
        public SpriteAnimator SpriteAnimator;
        public Sprite[] IdleAnimationSprites;
        public float IdleAnimationDuration = 1f;
        public Sprite[] RotationAnimationSprites;
        public float RotationAnimationDuration = 0.5f;
        
        [Header("Timer")]
        public float PassingTurnDuration = 3f;
        public TextMeshProUGUI TimerText;
        public TextMeshProUGUI SkipText;

        public float TurnDuration => DojoSessionManager.TurnDuration;
        private ulong _startTurnTime;
        private float _currentTurnTime => GetTurnTime();
        
        [Header("Events")]
        public UnityEvent OnClientLocalPlayerTurnEnd;
        public UnityEvent OnClientOpponentPlayerTurnEnd;
        public UnityEvent OnLocalPlayerTurnEnd;
        public UnityEvent OnOpponentPlayerTurnEnd;
        
        private Coroutine _clientTimerCoroutine;
        private Coroutine _passingTimerCoroutine;
        
        private bool _isLocalPlayerTurn = true;
        private bool _isTimerActive = false;
        
        

        public IEnumerator UpdateClientTimer()
        {
            if (!_isTimerActive) yield break;
            while (_currentTurnTime > 0)
            {
                TimerText.text = $"{Mathf.FloorToInt(_currentTurnTime / 60):00}:{Mathf.FloorToInt(_currentTurnTime % 60):00}";

                if (_currentTurnTime + PassingTurnDuration <= TurnDuration  / 3f)
                {
                    TimerText.color = new Color(0.866f, 0.08f, 0.236f);
                }
                UpdateTurnText(_isLocalPlayerTurn ? LocalPlayerTurnText : OpponentPlayerTurnText);
                yield return null;
            }
            TimerText.text = "";
            TimerText.color = Color.white;
            
            if(_passingTimerCoroutine != null)
            {
                StopCoroutine(_passingTimerCoroutine);
                _passingTimerCoroutine = null;
            }
            _passingTimerCoroutine = StartCoroutine(UpdatePassingTurnTimer());
            EndClientTimer();
        }
        
        public IEnumerator UpdatePassingTurnTimer()
        {
            if (_clientTimerCoroutine != null)
            {
                StopCoroutine(_clientTimerCoroutine);
                _clientTimerCoroutine = null;
            }
            float time = PassingTurnDuration;
            float timer = 0;
            
            while (timer < time)
            {
                timer += Time.deltaTime;
                UpdateTurnText(PassingTurnText);
                yield return null;
            }
            
            EndPassingTimer();
        }

        public void StartTurnTimer(ulong timestamp, bool isLocal)
        {
            ShowSkipText(false);
            RotateHourglass();
            
            // checking for the future
            ulong currentTimestamp = (ulong) (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            timestamp = timestamp > currentTimestamp ? currentTimestamp : timestamp;
            timestamp = timestamp == 0 ? currentTimestamp : timestamp;
            
            _isLocalPlayerTurn = isLocal;
            _startTurnTime = timestamp;
            _isTimerActive = true;
            TimerText.color = Color.white;
            
            if (_clientTimerCoroutine != null)
            {
                StopCoroutine(_clientTimerCoroutine);
                _clientTimerCoroutine = null;
            }
            if (_passingTimerCoroutine != null)
            {
                StopCoroutine(_passingTimerCoroutine);
                _passingTimerCoroutine = null;
            }
            _clientTimerCoroutine = StartCoroutine(UpdateClientTimer());
        }
        
        private float GetTurnTime()
        {
            ulong unixTimestamp = (ulong) (System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            float turnTime = TurnDuration - (unixTimestamp - _startTurnTime) - PassingTurnDuration;
            return turnTime <= 0 ? 0 : turnTime;
        }
        

        private void EndClientTimer()
        {
            _isTimerActive = false;
            CustomLogger.LogInfo("End client timer");
            if (_isLocalPlayerTurn)
            {
                CustomLogger.LogInfo("Local player client turn end");
                OnClientLocalPlayerTurnEnd?.Invoke();
            }
            else
            {
                CustomLogger.LogInfo("Opponent player client turn end");
                OnClientOpponentPlayerTurnEnd?.Invoke();
            }
        }

        private void EndPassingTimer()
        {
            _isTimerActive = false;
            CustomLogger.LogInfo("End timer");
            if (_isLocalPlayerTurn)
            {
                CustomLogger.LogInfo("Local player turn end");
                OnLocalPlayerTurnEnd?.Invoke();
                ShowSkipText(true);
            }
            else
            {
                CustomLogger.LogInfo("Opponent player turn end");
                OnOpponentPlayerTurnEnd?.Invoke();
                ShowSkipText(true);
            }
        }

        public void ShowSkipText(bool isActive)
        {
            if (isActive)
            {
                Sequence sequence = DOTween.Sequence();
                Color skipTextColor = SkipText.color;
                skipTextColor.a = 0f;
                SkipText.color = skipTextColor;
                SkipText.gameObject.SetActive(true);
                sequence.Append(SkipText.DOFade(1f, 0.5f));
                sequence.AppendInterval(5f);
                sequence.Append(SkipText.DOFade(0f, 0.5f)).OnComplete(() =>
                {
                    SkipText.gameObject.SetActive(false);
                });
                
                sequence.Play();
            }
            else
            {
                SkipText.gameObject.SetActive(false);
            }
        }
        
        private void RotateHourglass()
        {
            SpriteAnimator.duration = RotationAnimationDuration;
            SpriteAnimator.Play(RotationAnimationSprites).OnComplete(
                () =>
                {
                    SpriteAnimator.duration = IdleAnimationDuration;
                    SpriteAnimator.Play(IdleAnimationSprites);
                });
        }
        
        private void UpdateTurnText(string baseText)
        {
            int visibleDots = 3 - ((int)(_currentTurnTime % 3));
            string dots = string.Join("", new string[3].Select((_, index) => 
                $"<color=#{(index < visibleDots ? "FFFFFFFF" : "FFFFFF00")}>.</color>"));
            
            TurnText.text = baseText + dots;
        }

        public void SetActiveTimer(bool active)
        {
            gameObject.SetActive(active);
        }

    }
}