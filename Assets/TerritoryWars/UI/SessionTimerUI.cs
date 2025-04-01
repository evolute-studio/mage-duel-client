using System.Linq;
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
        [Tooltip(" @ - player name")]
        public string OpponentPlayerTurnText = $"Waiting for @ turn";
        
        [Header("Hourglass")]
        public SpriteAnimator SpriteAnimator;
        public Sprite[] IdleAnimationSprites;
        public float IdleAnimationDuration = 1f;
        public Sprite[] RotationAnimationSprites;
        public float RotationAnimationDuration = 0.5f;
        
        [Header("Timer")]
        public TextMeshProUGUI TimerText;
        public float TurnDuration = 120f;
        private float _startTurnTime;
        private float _currentTurnTime => GetTurnTime();
        
        [Header("Events")]
        public UnityEvent OnLocalPlayerTurnEnd;
        public UnityEvent OnOpponentPlayerTurnEnd;
        
        private bool _isLocalPlayerTurn => SessionManager.Instance.IsLocalPlayerTurn;
        private bool _isTimerActive = false;
        private string _opponentPlayerName = "opponent's";
        
        

        public void Update()
        {
            UpdateTimer();
            UpdateTurnText();
        }

        public void StartTurnTimer()
        {
            RotateHourglass();
            _startTurnTime = Time.time;
            _isTimerActive = true;
            TimerText.color = Color.white;
        }
        
        private void UpdateTimer()
        {
            if (!_isTimerActive) return;
            
            TimerText.text = $"{Mathf.FloorToInt(_currentTurnTime / 60):00}:{Mathf.FloorToInt(_currentTurnTime % 60):00}";

            if (_currentTurnTime <= TurnDuration / 4)
            {
                TimerText.color = new Color(0.866f, 0.08f, 0.236f);
            }
            
            if (_currentTurnTime <= 0)
            {
                TimerText.text = "00:00";
                EndTimer();
            }
        }
        
        private float GetTurnTime()
        {
            float turnTime = TurnDuration - (Time.time - _startTurnTime);
            if (turnTime < 0.1f)
            {
                turnTime = 0;
            }
            return turnTime < 0 ? 0 : turnTime;
        }
        
        private void EndTimer()
        {
            _isTimerActive = false;
            CustomLogger.LogInfo("End timer");
            if (_isLocalPlayerTurn)
            {
                CustomLogger.LogInfo("Local player turn end");
                OnLocalPlayerTurnEnd?.Invoke();
            }
            else
            {
                CustomLogger.LogInfo("Opponent player turn end");
                OnOpponentPlayerTurnEnd?.Invoke();
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
        
        private void UpdateTurnText()
        {
            string baseText;
            if (_isLocalPlayerTurn)
            {
                baseText = LocalPlayerTurnText;
            }
            else
            {
                baseText = OpponentPlayerTurnText.Replace("@", _opponentPlayerName);
            }
            
            int visibleDots = 3 - ((int)(_currentTurnTime % 3));
            string dots = string.Join("", new string[3].Select((_, index) => 
                $"<color=#{(index < visibleDots ? "FFFFFFFF" : "FFFFFF00")}>.</color>"));
            
            TurnText.text = baseText + dots;
        }

    }
}