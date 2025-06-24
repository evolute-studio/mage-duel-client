using System.Collections;
using System.Linq;
using DG.Tweening;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.Managers.SessionComponents;
using TerritoryWars.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using TerritoryWars.DataModels;
using TerritoryWars.DataModels.ClientEvents;

namespace TerritoryWars.UI.Session
{
    public class SessionTimer : MonoBehaviour
    {
        [Header("Turn text info")]
        public TextMeshProUGUI TurnText;
        public TextMeshProUGUI TimerText;
        public TextMeshProUGUI SkipText;
        
        public string GameCreationText = "Game is being created";
        public string GameRevealingText = "Revealing tile";
        public string GameRequestText = "Requesting tile";
        
        public string LocalPlayerTurnText = "Your turn now";
        public string OpponentPlayerTurnText = "Waiting for opponent's turn";
        public string PassingTurnText = "Passing the turn";

        [Header("Hourglass")]
        public SpriteAnimator SpriteAnimator;
        public Sprite[] IdleAnimationSprites;
        public float IdleAnimationDuration = 1f;
        public Sprite[] RotationAnimationSprites;
        public float RotationAnimationDuration = 0.5f;

        [Header("Timer")]
        public float TurnDuration => GameConfiguration.TurnDuration;
        public float PassingTurnDuration => GameConfiguration.PassingTurnDuration;
        private float _opponentReducedTime = 0.25f;
        private TimerEventType _timerType;

        // Local variables
        private bool _isLocalPlayerTurn => SessionManager.Instance.SessionContext.IsLocalPlayerTurn;
        private ulong _startTurnTimestamp;
        private float _timeGone => GetTurnTime();
        private Coroutine _timerCoroutine;

        public void Awake()
        {
            EventBus.Subscribe<TimerEvent>(OnTimerEvent);
        }

        private void OnTimerEvent(TimerEvent timerEvent)
        {
            _timerType = timerEvent.Type;
            if(timerEvent.ProgressType == TimerProgressType.Started)
                StartTimer(timerEvent.StartTimestamp);
        }

        public void StartTimer(ulong timestamp)
        {
            _startTurnTimestamp = timestamp;

            RotateHourglass();
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
            _timerCoroutine = StartCoroutine(UpdateTimer());
        }

        private IEnumerator UpdateTimer()
        {
            // main loop for the timer
            while (_timeGone < TurnDuration)
            {
                UpdateMainLoopTimer();
                yield return new WaitForSeconds(0.5f);
            }
            ResetUI();
            EventBus.Publish(new TimerEvent(_timerType, TimerProgressType.Elapsed));

            // passing turn
            float reducer = !_isLocalPlayerTurn ? _opponentReducedTime : 0;
            while (_timeGone < TurnDuration + PassingTurnDuration - reducer)
            {
                UpdateTurnText(GetTimerText());
                yield return null;
            }
            ResetUI();
        }

        private void UpdateMainLoopTimer()
        {
            float timeLeft = TurnDuration - _timeGone;
            TimerText.text = $"{Mathf.FloorToInt(timeLeft / 60):00}:{Mathf.FloorToInt(timeLeft % 60):00}";
            if (timeLeft <= TurnDuration / 3f)
                TimerText.color = new Color(0.866f, 0.08f, 0.236f); // Red color

            else
                TimerText.color = Color.white; // Default color
            UpdateTurnText(GetTimerText());
        }

        private float GetTurnTime()
        {
            ulong unixTimestamp = (ulong)(System.DateTime.UtcNow.Subtract(new System.DateTime(1970, 1, 1))).TotalSeconds;
            return (float)(unixTimestamp - _startTurnTimestamp);
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
        
        private string GetTimerText()
        {
            switch (_timerType)
            {
                case TimerEventType.GameCreation:
                    return GameCreationText;
                case TimerEventType.Revealing:
                    return GameRevealingText;
                case TimerEventType.Requesting:
                    return GameRequestText;
                case TimerEventType.Moving:
                    return _isLocalPlayerTurn ? LocalPlayerTurnText : OpponentPlayerTurnText;
                default:
                    return "";
            }
        }

        private void UpdateTurnText(string baseText)
        {
            int visibleDots = 3 - ((int)(_timeGone % 3));
            string dots = string.Join("", new string[3].Select((_, index) =>
                $"<color=#{(index < visibleDots ? "FFFFFF" : "FFFF00")}>.</color>"));

            TurnText.text = baseText + dots;
        }

        private void ResetUI()
        {
            TurnText.text = "";
            TimerText.text = "";
            SkipText.text = "";
            TimerText.color = Color.white;
            TurnText.color = Color.white;
            //SpriteAnimator.Stop();
        }

        public void OnDestroy()
        {
            if (_timerCoroutine != null)
            {
                StopCoroutine(_timerCoroutine);
            }
            EventBus.Unsubscribe<TimerEvent>(OnTimerEvent);
        }
    }
}
