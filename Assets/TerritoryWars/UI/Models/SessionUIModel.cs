using TerritoryWars.General;
using TerritoryWars.Tools;
using UnityEngine.UI;

public class SessionUIModel
{
    public System.Action OnValuesChanged;
    
    public CharactersObject charactersObject;
    public float TimeForTurn { get; set; } = 600f;

    public int[] CityScores
    {
        get { return CityScores; }
        set
        {
            CityScores = value;
            OnValuesChanged?.Invoke();
        }
    }

    public int[] TileScores
    {
        get { return TileScores; }
        set
        {
            TileScores = value;
            OnValuesChanged.Invoke();
        } 
    }
    
    public int[] Scores
    {
        get { return Scores;}
        set
        {
            Scores = value;
            OnValuesChanged.Invoke();
        } 
    }
    
    public string[] PlayerNames
    {
        get { return PlayerNames;}
        set
        {
            PlayerNames = value;
            OnValuesChanged.Invoke();
        } 
    }
    
    public int[] JokerCount
    {
        get { return JokerCount;}
        set
        {
            JokerCount = value;
            OnValuesChanged.Invoke();
        } 
    }
    
    public Image[] PlayerAvatars
    {
        get { return PlayerAvatars;}
        set
        {
            PlayerAvatars = value;
            OnValuesChanged.Invoke();
        } 
    } 

    public SessionUIModel(float timeForTurn, string[] playerNames, Image[] playerAvatars)
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