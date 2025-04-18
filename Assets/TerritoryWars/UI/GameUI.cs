using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TerritoryWars.UI
{
    public class GameUI : MonoBehaviour
    {
        public static GameUI Instance { get; private set; }
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        
        [Header("References")]
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Button skipTurnButton;
        [SerializeField] private Button rotateTileButton;
        [SerializeField] private Image currentTilePreview;
        [SerializeField] private Button jokerButton;
        [SerializeField] private Button deckButton;
        [SerializeField] private GameObject jokerModeIndicator;
        [SerializeField] private Sprite[] _toggleMods;
        [SerializeField] private Image _toggleSpriteRenderer;
        [SerializeField] private GameObject[] _toggleGameObjects;
        [SerializeField] private GameObject _toggleJokerButton;
        [SerializeField] private GameObject[] _toggleJokerInfoGameObjects;
        [SerializeField] private CanvasGroup[] _togglersCanvasGroup;
        [SerializeField] private CanvasGroup _deckContainerCanvasGroup;

        [SerializeField] private ResultPopUpUI _resultPopUpUI;
        [FormerlySerializedAs("SessionUI")] [SerializeField] public PlayerInfoUI playerInfoUI;
        
        [SerializeField] private Button SaveSnapshotButton;
        [SerializeField] private TextMeshProUGUI SaveSnapshotText;
        
        public static event Action OnJokerButtonClickedEvent;

        [Header("Tile Preview")]
        [SerializeField] private TilePreview tilePreview;
        [SerializeField] private TileJokerAnimator tilePreviewUITileJokerAnimator;

        private SessionManager _sessionManager;
        private DeckManager deckManager;
        
        private TweenerCore<Vector3,Vector3,VectorOptions> _skipButtonTween;
        private TweenerCore<Vector3,Vector3,VectorOptions> _jokerButtonTween;
        
        private Vector3 _skipButtonScale;
        
        [SerializeField] private ArrowAnimations arrowAnimations;
        private bool _isJokerActive = false;

        public void Initialize()
        {
            _sessionManager = FindObjectOfType<General.SessionManager>();
            deckManager = FindObjectOfType<DeckManager>();

            SetupButtons();
            UpdateUI();
            
            
            if (jokerModeIndicator != null)
            {
                jokerModeIndicator.SetActive(false);
            }
            _skipButtonScale = skipTurnButton.transform.localScale;
        }

        private void SetupButtons()
        {
            if (rotateTileButton != null)
            {
                rotateTileButton.onClick.AddListener(OnRotateButtonClicked);
                Debug.Log("Rotate button listener added");
            }
            else
            {
                Debug.LogError("Rotate button reference is missing!");
            }

            if (endTurnButton != null)
            {
                endTurnButton.onClick.AddListener(OnEndTurnClicked);
            }
            
            if (skipTurnButton != null)
            {
                skipTurnButton.onClick.AddListener(SkipMoveButtonClicked);
            }

            if (jokerButton != null)
            {
                jokerButton.onClick.AddListener(OnJokerButtonClicked);
            }
            
            if (deckButton != null)
            {
                deckButton.onClick.AddListener(OnDeckButtonClicked);
            }
            
            if (SaveSnapshotButton != null)
            {
                SaveSnapshotText.gameObject.SetActive(false);
                SaveSnapshotButton.onClick.AddListener(OnSaveSnapshotButtonClicked);
            }
        }
        
        public void ShowResultPopUp()
        {
            _resultPopUpUI.SetupButtons();
            if(_sessionManager.PlayersData[0] == null || _sessionManager.PlayersData[1] == null)
            {
                CustomLogger.LogWarning("PlayersData is null");
                return;
            }
            _resultPopUpUI.SetPlayersName(_sessionManager.PlayersData[0].username, _sessionManager.PlayersData[1].username);
            evolute_duel_Board board = DojoGameManager.Instance.DojoSessionManager.LocalPlayerBoard;
            int cityScoreBlue = board.blue_score.Item1;
            int cartScoreBlue = board.blue_score.Item2;
            int cityScoreRed = board.red_score.Item1;
            int cartScoreRed = board.red_score.Item2;
            int score1 = cityScoreBlue + cartScoreBlue + _sessionManager.Players[0].JokerCount * 5;
            int score2 = cityScoreRed + cartScoreRed + _sessionManager.Players[1].JokerCount * 5;
            _resultPopUpUI.SetPlayersScore(score1, score2);
            _resultPopUpUI.SetPlayersCityScores(cityScoreBlue, cityScoreRed);
            _resultPopUpUI.SetPlayersCartScores(cartScoreBlue, cartScoreRed);
            _resultPopUpUI.SetPlayersAvatars(playerInfoUI.charactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId()),
                playerInfoUI.charactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
            _resultPopUpUI.SetPlayerHeroAnimator(playerInfoUI.charactersObject.GetAnimatorController(PlayerCharactersManager.GetCurrentCharacterId()),
                playerInfoUI.charactersObject.GetAnimatorController(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
                
            bool isLocalPlayerBlue = SessionManager.Instance.LocalPlayer.LocalId == 0;
            string wonText;
            if (score1 > score2 && isLocalPlayerBlue || score1 < score2 && !isLocalPlayerBlue)
                wonText = "You won!";
            else if (score1 < score2 && isLocalPlayerBlue || score1 > score2 && !isLocalPlayerBlue)
                wonText = "You lose!";
            else
                wonText = "Draw!";
            _resultPopUpUI.SetWinnerText(wonText);
            _resultPopUpUI.SetPlayersJoker(board.GetJokerCountPlayer1(), board.GetJokerCountPlayer2());
            _resultPopUpUI.SetResultPopupActive(true);
            _resultPopUpUI.ViewResults();
        }

        public void UpdateUI()
        {
            if (tilePreview != null)
            {
                tilePreview.UpdatePreview(_sessionManager.TileSelector.CurrentTile);
            }

       
            if (jokerButton != null)
            {
                jokerButton.interactable = _sessionManager.JokerManager.CanUseJoker();
            }

           
            if (jokerModeIndicator != null)
            {
                jokerModeIndicator.SetActive(_sessionManager.JokerManager.IsJokerActive);
            }
        }

        private void OnEndTurnClicked()
        {
            SetEndTurnButtonActive(false);
            _sessionManager.EndTurn();
            UpdateUI();
            SetActiveDeckContainer(false);
        }
        
        private void SkipMoveButtonClicked()
        {
            _sessionManager.ClientLocalPlayerSkip();
            SetActiveSkipButtonPulse(false);
            JokerButtonPulse(false);
            UpdateUI();
        }

        public void SetActiveSkipButtonPulse(bool isActive)
        {
            if (isActive)
            {
                skipTurnButton.transform.localScale = _skipButtonScale;
                _skipButtonTween = skipTurnButton.transform.DOScale(skipTurnButton.transform.localScale.x * 1.1f, 0.8f).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                _skipButtonTween?.Kill();
                skipTurnButton.transform.localScale = _skipButtonScale;
            }
        }
        
        public void SetActiveDeckContainer(bool active)
        {
            if (active)
            {
                // _togglersCanvasGroup[0].alpha = 0.5f;
                // _togglersCanvasGroup[0].DOFade(1, 0.5f);
                // _togglersCanvasGroup[1].alpha = 0.5f;
                // _togglersCanvasGroup[1].DOFade(1, 0.5f);
                _deckContainerCanvasGroup.alpha = 0.33f;
                _deckContainerCanvasGroup.DOFade(1, 0.5f);
                jokerButton.interactable = true;
                deckButton.interactable = true;
                
            }
            else
            {
                // _togglersCanvasGroup[0].alpha = 1f;
                // _togglersCanvasGroup[0].DOFade(0.5f, 0.5f);
                // _togglersCanvasGroup[1].alpha = 1f;
                // _togglersCanvasGroup[1].DOFade(0.5f, 0.5f);
                _deckContainerCanvasGroup.alpha = 1;
                _deckContainerCanvasGroup.DOFade(0.33f, 0.5f);
                jokerButton.interactable = false;
                deckButton.interactable = false;
            }
        }
        
        public void JokerButtonPulse(bool isActive)
        {
            if (isActive)
            {
                _toggleJokerButton.transform.localScale = Vector3.one;
                _toggleJokerInfoGameObjects[0].transform.localScale = Vector3.one;
                _toggleJokerInfoGameObjects[1].transform.localScale = Vector3.one;
                _jokerButtonTween = _toggleJokerButton.transform.DOScale(_toggleJokerButton.transform.localScale.x * 1.1f, 0.8f).SetLoops(-1, LoopType.Yoyo);
                _toggleJokerInfoGameObjects[0].transform.DOScale(_toggleJokerInfoGameObjects[0].transform.localScale.x * 1.1f, 0.8f).SetLoops(-1, LoopType.Yoyo);
                _toggleJokerInfoGameObjects[1].transform.DOScale(_toggleJokerInfoGameObjects[1].transform.localScale.x * 1.1f, 0.8f).SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                _toggleJokerInfoGameObjects[0].transform.DOKill();
                _toggleJokerInfoGameObjects[1].transform.DOKill();
                _jokerButtonTween?.Kill();
                _toggleJokerButton.transform.localScale = Vector3.one;
            }
        }

        private void OnRotateButtonClicked()
        {
            Debug.Log("Rotate button clicked");
            arrowAnimations.PlayRotationAnimation();
            _sessionManager.RotateCurrentTile();
        }

        public void OnJokerButtonClicked() 
        {
            if(SessionManager.Instance.CurrentTurnPlayer.JokerCount <= 0 || _isJokerActive || !SessionManager.Instance.IsLocalPlayerTurn) return;
            _isJokerActive = true;
            JokerButtonPulse(false);
            
            SetJokerMode(true);
            OnJokerButtonClickedEvent?.Invoke();
            
        }
        
        private void OnDeckButtonClicked()
        {
            if (!_isJokerActive) return;
            _isJokerActive = false;
            SetJokerMode(false);
        }
        
        public void SetJokerMode(bool active)
        {
            _isJokerActive = active;
            if (active)
            {
                _toggleGameObjects[1].SetActive(true);
                _toggleGameObjects[0].SetActive(false);
                
                _sessionManager.JokerManager.ActivateJoker();
                
                tilePreview._tileJokerAnimator.ShowIdleJokerAnimation();
                tilePreviewUITileJokerAnimator.ShowIdleJokerAnimation();
                UpdateUI();
            }
            else
            {
                _toggleGameObjects[1].SetActive(false);
                _toggleGameObjects[0].SetActive(true);
                _sessionManager.JokerManager.DeactivateJoker();
                tilePreview._tileJokerAnimator.StopIdleJokerAnimation();
                tilePreviewUITileJokerAnimator.StopIdleJokerAnimation();
                SessionManager.Instance.TileSelector.CancelJokerMode();
                UpdateUI();
            }
        }
        
        private void OnSaveSnapshotButtonClicked()
        {
            SaveSnapshotText.gameObject.SetActive(true);

            DojoGameManager.Instance.DojoSessionManager.CreateSnapshot();

            SaveSnapshotText.DOFade(1, 0.2f);
            DOVirtual.DelayedCall(3, () =>
            {
               
                SaveSnapshotText.DOFade(0, 0.5f).OnComplete(() =>
                {
                    SaveSnapshotText.gameObject.SetActive(false);
                });
            });
        }

        public void SetEndTurnButtonActive(bool active)
        {
            if (endTurnButton != null)
            {
                endTurnButton.gameObject.SetActive(active);
            }
        }
        
        public void SetSkipTurnButtonActive(bool active)
        {
            if (skipTurnButton != null)
            {
                skipTurnButton.gameObject.SetActive(active);
            }
        }

        public void SetRotateButtonActive(bool active)
        {
            if (rotateTileButton != null)
            {
                rotateTileButton.gameObject.SetActive(active);
                arrowAnimations.gameObject.SetActive(active);
                arrowAnimations.SetActiveArrow(active);
            }
        }

        private void SwitchToggle()
        {
            
        }

        public void SetJokerButtonActive(bool active)
        {
            if (jokerButton != null)
            {
                jokerButton.gameObject.SetActive(active);
            }
            
        }
    }
}