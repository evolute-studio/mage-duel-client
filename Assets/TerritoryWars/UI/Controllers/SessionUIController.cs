using System;
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
    
    
    private SessionManager _sessionManager;

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

    public void Intialization()
    {
        _sessionManager = FindObjectOfType<SessionManager>();
        SetupButtons();
        _model = new SessionUIModel(_timeForTurn, GetPlayerNames(), GetPlayerAvatars());
    }

    public void SetupButtons()
    {
        _view.rotateTileButton.onClick.AddListener(OnRotateButtonClicked);
        _view.endTurnButton.onClick.AddListener(OnEndTurnClicked);
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

    public void UpdateView()
    {
        _view.UpdateAvatarsDisplay(_model.PlayerAvatars);
        _view.UpdateScoresDisplay(_model.Scores);
        _view.UpdateCityScoreDisplay(_model.CityScores);
        _view.UpdateTileScoreDisplay(_model.TileScores);
        _view.UpdatePlayerNamesDisplay(_model.PlayerNames);
        _view.UpdateDeckCountDisplay(_model.DeckCount);
    }
    
    public void UpdateTilePreview()
    {
        _view.UpdateTilePreview();
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
        _view.PlayArrowAnimation();
        _model.RotateCurrentTile();
    }

    private void OnSaveSnapshotButtonClicked()
    {
        DojoGameManager.Instance.SessionManager.CreateSnapshot();
        _view.OnSnapshotButtonClicked();
    }

    public void SetActiveDeckContainer(bool active)
    {
        _view.UpdateDeckContainerActive(active);
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
        _view.SetEndTurnButtonActive(active);
    }
    
    public void SetSkipTurnButtonActive(bool active)
    {
        _view.SetSkipTurnButtonActive(active);
    }

    public void SkipMoveButtonClicked()
    {
        _sessionManager.SkipMove();
        UpdateTilePreview();
    }
    
    public void SetRotateButtonActive(bool active)
    {
        _view.SetRotateButtonActive(active);
    }

    public void OnEndTurnClicked()
    {
        _view.OnEndTurnClicked();
        _sessionManager.EndTurn();
        UpdateTilePreview();
    }

    public void SetJokerMode(bool active)
    {
        _model.IsJokerMode = active;
        if (active)
        {
            _view.SetJokerModeAnimation(active);
            _sessionManager.JokerManager.ActivateJoker();
        }
        else
        {
            _view.SetJokerModeAnimation(active);
            _sessionManager.JokerManager.DeactivateJoker();
            _sessionManager.TileSelector.CancelJokerMode();
        }
    }

}
