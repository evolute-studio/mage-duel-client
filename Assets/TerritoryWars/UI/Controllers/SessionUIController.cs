using System;
using JetBrains.Annotations;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.UI;
using UnityEngine;

public class SessionUIController : MonoBehaviour
{
    public static SessionUIController Instance { get; private set; }
    
    private SessionUIModel _model;
    [SerializeField] SessionUIView _view;
    [SerializeField] private ResultPopUpUI _resultPopUpUI;
    [SerializeField] private float _timeForTurn;
    [SerializeField] public SessionTimerUI SessionTimerUI;
    [SerializeField] private CancelGamePopup _cancelGamePopup;
    
    
    private SessionManager _sessionManager;


    public bool useViews;

    private void Awake()
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

    public void Intialization(bool useViews)
    {
        this.useViews = useViews;
        _sessionManager = FindObjectOfType<SessionManager>();
        
        SetupButtons();
        UpdateTilePreview();
        
        _model = new SessionUIModel(_timeForTurn, GetPlayerNames(), GetPlayerAvatars());
        _model.OnValuesChanged += UpdateView;
    }
    
    public T UseView<T>(T view) where T : IView
    {
        if (useViews && view != null)
        {
            return view;
        }

        return default;
    }

    public void SetupButtons()
    {
        UseView(_view)?.rotateTileButton.onClick.AddListener(OnRotateButtonClicked);
        UseView(_view)?.rotateTileButton.onClick.AddListener(OnRotateButtonClicked);
        UseView(_view)?.endTurnButton.onClick.AddListener(OnEndTurnClicked);
        UseView(_view)?.skipTurnButton.onClick.AddListener(SkipMoveButtonClicked);
        UseView(_view)?.jokerButton.onClick.AddListener(OnJokerButtonClicked);
        UseView(_view)?.deckButton.onClick.AddListener(OnDeckButtonClicked);
        UseView(_view)?.SaveSnapshotText.gameObject.SetActive(false);
        UseView(_view)?.SaveSnapshotButton.onClick.AddListener(OnSaveSnapshotButtonClicked);
        UseView(_view)?.CancelGameButton.onClick.AddListener(OnCancelGameClicked);
    }
    public void UpdateView()
    {
        UseView(_view)?.UpdateAvatarsDisplay(_model.PlayerAvatars);
        UseView(_view)?.UpdateScoresDisplay(_model.Scores);
        UseView(_view)?.UpdateCityScoreDisplay(_model.CityScores);
        UseView(_view)?.UpdateTileScoreDisplay(_model.TileScores);
        UseView(_view)?.UpdatePlayerNamesDisplay(_model.PlayerNames);
        UseView(_view)?.UpdateJokersCountDisplay(_model.JokerCount[0], _model.JokerCount[1]);
        UseView(_view)?.UpdateJokerCountTextDisplay(_model.JokerCount[0]);
    }
    
    public void ShowResultPopUp()
    {
            _resultPopUpUI.SetupButtons();
            if(_sessionManager.PlayersData[0] == null || _sessionManager.PlayersData[1] == null)
            {
                return;
            }
            
            _resultPopUpUI.SetPlayersName(_sessionManager.PlayersData[0].username, _sessionManager.PlayersData[1].username);
            evolute_duel_Board board = DojoGameManager.Instance.SessionManager.LocalPlayerBoard;
            int cityScoreBlue = board.blue_score.Item1;
            int cartScoreBlue = board.blue_score.Item2;
            int cityScoreRed = board.red_score.Item1;
            int cartScoreRed = board.red_score.Item2;
            int score1 = cityScoreBlue + cartScoreBlue + _sessionManager.Players[0].JokerCount * 5;
            int score2 = cityScoreRed + cartScoreRed + _sessionManager.Players[1].JokerCount * 5;
            _resultPopUpUI.SetPlayersScore(score1, score2);
            _resultPopUpUI.SetPlayersCityScores(cityScoreBlue, cityScoreRed);
            _resultPopUpUI.SetPlayersCartScores(cartScoreBlue, cartScoreRed);
            // _resultPopUpUI.SetPlayersAvatars(playerInfoUI.charactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId()),
            //     playerInfoUI.charactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
            // _resultPopUpUI.SetPlayerHeroAnimator(playerInfoUI.charactersObject.GetAnimatorController(PlayerCharactersManager.GetCurrentCharacterId()),
            //     playerInfoUI.charactersObject.GetAnimatorController(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
                
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

    
    public void UpdateTilePreview()
    {
        UseView(_view)?.UpdateTilePreview();
    }

    public void ChangeScores(int[] playerScores)
    {
        int[] scores = SetLocalPlayerData.GetLocalPlayerInt(playerScores[0], playerScores[1]);
        _model.Scores = scores;
    }
    
    public void ChangeCityScores(int[] playerCityScores)
    {
        int[] scores = SetLocalPlayerData.GetLocalPlayerInt(playerCityScores[0], playerCityScores[1]);
        _model.CityScores = scores;
    }
    

    public void ChangeTilesScores(int[] playerTileScores)
    {
        int[] scores = SetLocalPlayerData.GetLocalPlayerInt(playerTileScores[0], playerTileScores[1]);
        _model.TileScores = scores;
    }

    public void ChangeJokerCount(int[] playerJokerCount)
    {
        int[] jokerCount = SetLocalPlayerData.GetLocalPlayerInt(playerJokerCount[0], playerJokerCount[1]);
        _model.JokerCount = jokerCount;
    }

    public string[] GetPlayerNames()
    {
        string[] playerNames = SetLocalPlayerData.GetLocalPlayerString(_sessionManager.PlayersData[0].username, _sessionManager.PlayersData[1].username);
        return playerNames;
    }

    public Sprite[] GetPlayerAvatars()
    {
        Sprite[] playerAvatars = new []{PrefabsManager.Instance.CharactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId()),
            PrefabsManager.Instance.CharactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId())};
        return playerAvatars;
    }

    public void OnRotateButtonClicked()
    {
        UseView(_view)?.PlayArrowAnimation();
        _model.RotateCurrentTile();
    }

    public void SetDeckCount(int count)
    {
        UseView(_view)?.UpdateDeckCountDisplay(count);
    }

    private void OnSaveSnapshotButtonClicked()
    {
        DojoGameManager.Instance.SessionManager.CreateSnapshot();
        UseView(_view)?.OnSnapshotButtonClicked();
    }

    public void SetActiveDeckContainer(bool active)
    {
        UseView(_view)?.UpdateDeckContainerActive(active);
    }

    public void OnDeckButtonClicked()
    {
        if(!_model.IsJokerMode)
            return;
        
        _model.IsJokerMode = false;
        SetJokerMode(false);
    }
    
    public void OnJokerButtonClicked()
    {
        if(_sessionManager.CurrentTurnPlayer.JokerCount <= 0 || _model.IsJokerMode || !_sessionManager.IsLocalPlayerTurn)
            return;
        
        _model.IsJokerMode = true;
        SetJokerMode(true);
    }
    
    public void SetEndTurnButtonActive(bool active)
    {
        UseView(_view)?.SetEndTurnButtonActive(active);
    }
    
    public void SetSkipTurnButtonActive(bool active)
    {
        UseView(_view)?.SetSkipTurnButtonActive(active);
    }

    public void SkipMoveButtonClicked()
    {
        _sessionManager.SkipMove();
        UpdateTilePreview();
    }
    
    public void SetRotateButtonActive(bool active)
    {
        UseView(_view)?.SetRotateButtonActive(active);
    }

    public void OnEndTurnClicked()
    {
        UseView(_view)?.OnEndTurnClicked();
        _sessionManager.EndTurn();
        UpdateTilePreview();
    }

    public void SetJokerMode(bool active)
    {
        _model.IsJokerMode = active;
        if (active)
        {
            UseView(_view)?.SetJokerModeAnimation(active);
            _sessionManager.JokerManager.ActivateJoker();
            UpdateTilePreview();
        }
        else
        {
            UseView(_view)?.SetJokerModeAnimation(active);
            _sessionManager.JokerManager.DeactivateJoker();
            _sessionManager.TileSelector.CancelJokerMode();
            UpdateTilePreview();
        }
    }

    public void OnCancelGameClicked()
    {
        _cancelGamePopup.SetActive(true);
    }
}
