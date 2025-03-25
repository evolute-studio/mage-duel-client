using System;
using System.Collections.Generic;
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
    public List<TextMeshProUGUI> tileScoreTextPlayers;
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
            tileScoreTextPlayers[i].text = players[i].tileScore.ToString();
            timeTextPlayers[i].text = players[i].time.ToString();
        }

        foreach (var player in players)
        {
            foreach (var joker in player.jokersImage)
            {
                joker.color = JokerAvailableColor;
            }
        }
        CancelGameButton.onClick.AddListener(() =>
        {
            CancelGamePopUp.SetActive(true);
        });
        
        SetPlayersAvatars(charactersObject.GetAvatar(PlayerCharactersManager.GetCurrentCharacterId()), 
            charactersObject.GetAvatar(PlayerCharactersManager.GetOpponentCurrentCharacterId()));
    }
    
    public void SetPlayersAvatars(Sprite localPlayerSprite, Sprite remotePlayerSprite)
    {
        players[0].playerImage.sprite = localPlayerSprite;
        players[1].playerImage.sprite = remotePlayerSprite;
    }
    

    public void SetCityScores(int localPlayerCityScore, int remotePlayerCityScore)
    {
        int[] playersCityScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerCityScore, remotePlayerCityScore);
        cityScoreTextPlayers[0].text = (playersCityScores[0] / 2).ToString();
        cityScoreTextPlayers[1].text = (playersCityScores[1] / 2).ToString();
    }
    
    
    public void SetRoadScores(int localPlayerTileScore, int remotePlayerTileScore)
    {
        int[] playersRoadScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerTileScore, remotePlayerTileScore);
        tileScoreTextPlayers[0].text = playersRoadScores[0].ToString();
        tileScoreTextPlayers[1].text = playersRoadScores[1].ToString();
    }

    public void SetPlayerScores(int localPlayerScore, int remotePlayerScore)
    {
        int[] playersScores = SetLocalPlayerData.GetLocalPlayerInt(localPlayerScore, remotePlayerScore);
        LocalPlayerScoreText.text = playersScores[0].ToString();
        RemotePlayerScoreText.text = playersScores[1].ToString();
    }
    
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
        public int cityScore = 0;
        public int tileScore = 0;
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
