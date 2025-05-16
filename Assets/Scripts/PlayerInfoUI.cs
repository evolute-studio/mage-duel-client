using System;
using System.Collections.Generic;
using DG.Tweening;
using TerritoryWars;
using TerritoryWars.General;
using TerritoryWars.Tile;
using TerritoryWars.Tools;
using TerritoryWars.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class PlayerInfoUI : MonoBehaviour
{
    public SessionTimerUI SessionTimerUI;
    
    public Color JokerNotAvailableColor;
    public Color JokerAvailableColor;
    public List<TextMeshProUGUI> cityScoreTextPlayers;
    [FormerlySerializedAs("tileScoreTextPlayers")] public List<TextMeshProUGUI> roadScoreTextPlayers;
    public List<TextMeshProUGUI> timeTextPlayers;
    [SerializeField] private Board _board;
    public CharactersObject charactersObject;
    public TextMeshProUGUI LocalPlayerName;
    public TextMeshProUGUI RemotePlayerName;
    public TextMeshProUGUI LocalPlayerScoreText;
    public TextMeshProUGUI RemotePlayerScoreText;
    public TextMeshProUGUI DeckCountText; 
    public Button CancelGameButton;
    
    public CancelGamePopup CancelGamePopUp;

    [Header("Players")]
    public List<PlayerInfo> players;
    
    private SessionManager _sessionManager;

    public void Initialization()
    {
        _sessionManager = FindObjectOfType<SessionManager>();
        SetNames(_sessionManager.PlayersData[0].username, _sessionManager.PlayersData[1].username);
        for (int i = 0; i < players.Count; i++)
        {
            cityScoreTextPlayers[i].text = players[i].cityScore.ToString();
            roadScoreTextPlayers[i].text = players[i].roadScore.ToString();
            timeTextPlayers[i].text = players[i].time.ToString();
        }

        foreach (var player in players)
        {
            foreach (var joker in player.jokersImage)
            {
                joker.color = JokerAvailableColor;
            }
        }
        
        SetPlayersAvatars(charactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId()), 
            charactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
    }
    
    public void SetPlayersAvatars(Sprite localPlayerSprite, Sprite remotePlayerSprite)
    {
        players[0].playerImage.sprite = localPlayerSprite;
        players[1].playerImage.sprite = remotePlayerSprite;
    }
    

    public void SetCityScores(int firstPlayerCityScore, int secondPlayerCityScore, bool withAnimation = true, float delay = 0)
    {
        int[] playersCityScores = SetLocalPlayerData.GetLocalPlayerInt(firstPlayerCityScore, secondPlayerCityScore);
        if(cityScoreTextPlayers[0].text != playersCityScores[0].ToString() && withAnimation)
        {
            ValueChangedAnimation(cityScoreTextPlayers[0], players[0].cityScore < playersCityScores[0], delay);
        }
        if(cityScoreTextPlayers[1].text != playersCityScores[1].ToString() && withAnimation)
        {
            ValueChangedAnimation(cityScoreTextPlayers[1], players[1].cityScore < playersCityScores[1], delay);
        }
        cityScoreTextPlayers[0].text = playersCityScores[0].ToString();
        cityScoreTextPlayers[1].text = playersCityScores[1].ToString();
        players[0].cityScore = playersCityScores[0];
        players[1].cityScore = playersCityScores[1];
    }

    public void AddClientCityScore(int serverSideId, int playerCityScore)
    {
        if(playerCityScore == 0) return;
        int localSideId = SetLocalPlayerData.GetLocalIndex(serverSideId);
        ValueChangedAnimation(cityScoreTextPlayers[localSideId], true, 0);
        players[localSideId].cityScore += playerCityScore;
        cityScoreTextPlayers[localSideId].text = players[localSideId].cityScore.ToString();
        SetPlayerScoresWithoutSwap(players[0].generalScore, players[1].generalScore);
    }
    
    
    public void SetRoadScores(int localPlayerTileScore, int remotePlayerTileScore, bool withAnimation = true, float delay = 0)
    {
        int[] playersRoadScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerTileScore, remotePlayerTileScore);
        if(roadScoreTextPlayers[0].text != playersRoadScores[0].ToString() && withAnimation)
        {
            ValueChangedAnimation(roadScoreTextPlayers[0], players[0].roadScore < playersRoadScores[0], delay);
        }
        if(roadScoreTextPlayers[1].text != playersRoadScores[1].ToString() && withAnimation)
        {
            ValueChangedAnimation(roadScoreTextPlayers[1], players[1].roadScore < playersRoadScores[1], delay);
        }
        roadScoreTextPlayers[0].text = playersRoadScores[0].ToString();
        roadScoreTextPlayers[1].text = playersRoadScores[1].ToString();
        players[0].roadScore = playersRoadScores[0];
        players[1].roadScore = playersRoadScores[1];
    }
    
    public void AddClientRoadScore(int playerIndex, int playerRoadScore)
    {
        if(playerRoadScore == 0) return;
        playerIndex = SetLocalPlayerData.GetLocalIndex(playerIndex);
        ValueChangedAnimation(roadScoreTextPlayers[playerIndex], true, 0);
        players[playerIndex].roadScore += playerRoadScore;
        roadScoreTextPlayers[playerIndex].text = players[playerIndex].roadScore.ToString();
        SetPlayerScoresWithoutSwap(players[0].generalScore, players[1].generalScore);
    }

    private void ValueChangedAnimation(TextMeshProUGUI text, bool isNewValueBigger, float delay)
    {
        text.transform.parent.DOKill();
        text.DOKill();
        Color color = isNewValueBigger ? Color.green : Color.red;
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.Append(text.transform.parent.DOScale(1.2f, 0.2f).SetEase(Ease.OutBack));
        seq.Join(text.DOColor(color, 0.2f).SetEase(Ease.OutBack));
        seq.Append(text.transform.parent.DOScale(1f, 0.3f).SetEase(Ease.InOutSine));
        
        seq.Append(text.transform.parent.DOScale(1.1f, 0.3f).SetEase(Ease.InOutSine));
        seq.Append(text.transform.parent.DOScale(1f, 0.3f).SetEase(Ease.InOutSine));
        seq.Append(text.transform.parent.DOScale(1.05f, 0.3f).SetEase(Ease.InOutSine));
        
        seq.Append(text.transform.parent.DOScale(1f, 0.5f).SetEase(Ease.OutExpo));
        seq.Join(text.DOColor(Color.white, 1f).SetEase(Ease.OutExpo));
        seq.Play();
    }
    public void SetPlayerScores(int localPlayerScore, int remotePlayerScore, bool withAnimation = true, float delay = 0)
    {
        int[] playersScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerScore, remotePlayerScore);
        SetPlayerScoresWithoutSwap(playersScores[0], playersScores[1], withAnimation, delay);
    }
    public void SetPlayerScoresWithoutSwap(int localPlayerScore, int remotePlayerScore, bool withAnimation = true, float delay = 0)
    {
        if(LocalPlayerScoreText.text != localPlayerScore.ToString() && withAnimation)
        {
            int value = int.Parse(LocalPlayerScoreText.text);
            ValueChangedAnimation(LocalPlayerScoreText, value <= localPlayerScore, delay);
        }
        if(RemotePlayerScoreText.text != remotePlayerScore.ToString() && withAnimation)
        {
            int value = int.Parse(RemotePlayerScoreText.text);
            ValueChangedAnimation(RemotePlayerScoreText, value <= remotePlayerScore, delay);
        }
        LocalPlayerScoreText.text = localPlayerScore.ToString();
        RemotePlayerScoreText.text = remotePlayerScore.ToString();
    }

    // public void GeneralScoreClientPrediction(int playerIndex)
    // {
    //     int localPlayerScore = players[0].cityScore + players[0].roadScore;
    //     int remotePlayerScore = players[1].cityScore + players[1].roadScore;
    //     int[] playersScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerScore, remotePlayerScore);
    //     if(LocalPlayerScoreText.text != playersScores[0].ToString())
    //     {
    //         ValueChangedAnimation(LocalPlayerScoreText);
    //     }
    //     if(RemotePlayerScoreText.text != playersScores[1].ToString())
    //     {
    //         ValueChangedAnimation(RemotePlayerScoreText);
    //     }
    //     LocalPlayerScoreText.text = playersScores[0].ToString();
    //     RemotePlayerScoreText.text = playersScores[1].ToString();
    // }
    
    public void SetNames(string localPlayerName, string remotePlayerName)
    {
        string[] playerNames = SetLocalPlayerData.GetLocalPlayerString(localPlayerName, remotePlayerName);
        LocalPlayerName.text = playerNames[0];
        RemotePlayerName.text = playerNames[1];
    }

    public void ShowPlayerJokerCount(int playerId)
    {
        if (!SessionManager.Instance.IsLocalPlayerHost)
        {
            playerId = playerId == 0 ? 1 : 0;
        }
        
        players[playerId].jokerCountText.text = players[playerId].jokerCount.ToString();
    }
    
    public void SetJokersCount(int player, int count)
    {
        if (!SessionManager.Instance.IsLocalPlayerHost)
        {
            player = player == 0 ? 1 : 0;
        }

        players[player].jokerCount = count;
        for (int i = 0; i < players[player].jokersImage.Count; i++)
        {
            //players[player].jokersImage[i].color = i < count ? JokerAvailableColor : JokerNotAvailableColor;
            players[player].jokersImage[i].GetComponent<Button>().interactable = i < count;
            if (players[player].jokersImage[i].TryGetComponent<CursorOnHover>(out var hover))
            {
                hover.enabled = i < count;
            }
            
        }
    }
    
    public void SetDeckCount(int count)
    {
        DeckCountText.text = count.ToString();
    }

    [Serializable]
    public class PlayerInfo
    {
        public Image playerImage;
        public int jokerCount = 3;
        public int generalScore => cityScore + roadScore;
        public int cityScore = 0;
        public int roadScore = 0;
        public float time = 600f;
        public List<Image> jokersImage;
        public TextMeshProUGUI jokerCountText;

        public void UpdateTimer()
        {
            time -= Time.deltaTime;
            if (time <= 0)
            {
                time = 0;
            }
        }
    }
}
