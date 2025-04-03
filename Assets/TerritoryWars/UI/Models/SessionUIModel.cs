using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine;
using UnityEngine.UI;

public class SessionUIModel
{
    public System.Action OnValuesChanged;
    
    public CharactersObject charactersObject;
    
    private bool _isJokerMode;
    public bool IsJokerMode
    {
        get { return _isJokerMode; }
        set
        {
            _isJokerMode = value;
            OnValuesChanged?.Invoke();
        }
    }
    
    public float TimeForTurn { get; set; } = 600f;

    private int[] _cityScores;
    public int[] CityScores
    {
        get { return _cityScores; }
        set
        {
            _cityScores = value;
            OnValuesChanged?.Invoke();
        }
    }

    private int[] _tileScores;
    public int[] TileScores
    {
        get { return _tileScores; }
        set
        {
            _tileScores = value;
            OnValuesChanged?.Invoke();
        } 
    }
    
    private int[] _scores;
    public int[] Scores
    {
        get { return _scores;}
        set
        {
            _scores = value;
            OnValuesChanged?.Invoke();
        } 
    }
    
    private string[] _playerNames;
    public string[] PlayerNames
    {
        get { return _playerNames;}
        set
        {
            _playerNames = value;
            OnValuesChanged?.Invoke();
        } 
    }
    
    private int[] _jokerCount;
    public int[] JokerCount
    {
        get { return _jokerCount;}
        set
        {
            _jokerCount = value;
            OnValuesChanged?.Invoke();
        } 
    }
    
    private Sprite[] _playerAvatars;
    public Sprite[] PlayerAvatars
    {
        get { return _playerAvatars;}
        set
        {
            _playerAvatars = value;
            OnValuesChanged?.Invoke();
        } 
    }

    private int _deckCount;
    public int DeckCount
    {
        get { return _deckCount; }
        set
        {
            _deckCount = value;
            OnValuesChanged?.Invoke();
        }
    }

    public SessionUIModel(float timeForTurn, string[] playerNames, Sprite[] playerAvatars)
    {
        CityScores = new int[] { 0, 0 };
        TileScores = new int[] { 0, 0 };
        Scores = new int[] { 0, 0 };
        JokerCount = new int[] { 3, 3 };
        TimeForTurn = timeForTurn;
        PlayerNames = playerNames;
        PlayerAvatars = playerAvatars;
    }

    public void RotateCurrentTile()
    {
        SessionManager.Instance.RotateCurrentTile();
    }

}