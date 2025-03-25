using System.Collections.Generic;
using Dojo;
using TerritoryWars.Dojo;
using TerritoryWars.General;
using TerritoryWars.ModelsDataConverters;
using TerritoryWars.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] private uint _playerToShow = 6;

    public GameObject PanelGameObject;
    public GameObject ListItemPrefab;
    [SerializeField] private Sprite[] _leadersImages;
    [SerializeField] private GameObject _placeHolder;
    [SerializeField] private Transform ListItemParent;


    private List<LeaderboardItem> _leaderboardItems = new List<LeaderboardItem>();
    private List<ModelInstance> _models = new List<ModelInstance>();
    private List<GameObject> _leaderboardObjects = new List<GameObject>();

    
    public void Start() => Initialize();

    public void Initialize()
    {
        
    }

    public LeaderboardItem CreateLeaderboardItem()
    {
        GameObject listItem = Instantiate(ListItemPrefab, ListItemParent);
        LeaderboardItem leaderboardItem = new LeaderboardItem(listItem);
        _leaderboardItems.Add(leaderboardItem);
        return leaderboardItem;
    }

    public void SetActivePanel(bool isActive)
    {
        // if (isActive && MenuUIController.Instance._namePanelController.IsDefaultName())
        // {
        //     MenuUIController.Instance._changeNamePanelUIController.SetNamePanelActive(true);
        //     return;
        // }
        // PanelGameObject.SetActive(isActive);
        PanelGameObject.SetActive(isActive);
        if (isActive)
        {
            ClearAllListItems();
            FetchData();
        }
    }

    private void ClearAllListItems()
    {
        foreach (var matchListItem in _leaderboardItems)
        {
            Destroy(matchListItem.ListItem);
        }

        _leaderboardItems.Clear();
    }

    public async void FetchData()
    {
        ApplicationState.SetState(ApplicationStates.Leaderboard);
        int count = await DojoGameManager.Instance.CustomSynchronizationMaster.SyncTopPlayersForLeaderboard(_playerToShow);
        ApplicationState.SetState(ApplicationStates.Menu);
        GameObject[] playersGO = DojoGameManager.Instance.WorldManager.Entities<evolute_duel_Player>();
        List<evolute_duel_Player> players = new List<evolute_duel_Player>();
        foreach (var player in playersGO)
        {
            players.Add(player.GetComponent<evolute_duel_Player>());
        }
        
        players.Sort((x, y) => y.balance.CompareTo(x.balance));
        evolute_duel_Player localPlayer = DojoGameManager.Instance.GetLocalPlayerData();
        // Destroy all players that are not in the top
        for (int i = (int)_playerToShow; i < players.Count; i++)
        {
            if (players[i].player_id.Hex() == localPlayer.player_id.Hex())
                continue;
            IncomingModelsFilter.DestroyModel(players[i]);
        }
        
        for (int i = 0; i < _playerToShow; i++)
        {
            if (i >= players.Count)
                break;
            LeaderboardItem leaderboardItem = CreateLeaderboardItem();
            leaderboardItem.PlayerName = CairoFieldsConverter.GetStringFromFieldElement(players[i].username);
            leaderboardItem.EvoluteCount = players[i].balance;
            leaderboardItem.SetLeaderPlace(i + 1);
            leaderboardItem.SetActive(true);
            leaderboardItem.UpdateItem();
        }
        
        //SetActivePlaceHolder(false);
    }

    public void SetActivePlaceHolder(bool isActive)
    {
        _placeHolder.SetActive(isActive);
    }
}


public class LeaderboardItem
{
        public GameObject ListItem;
        public string PlayerName;
        public int EvoluteCount;

        private TextMeshProUGUI _playerNameText;
        private TextMeshProUGUI _evoluteCount;
        private Image _leaderPlaceImage;

        public LeaderboardItem(GameObject listItem)
        {
            ListItem = listItem;
            _playerNameText = listItem.transform.Find("Name/NameText").GetComponent<TextMeshProUGUI>();
            _evoluteCount = listItem.transform.Find("EvoluteCount/Count/EvoluteCountText")
                .GetComponent<TextMeshProUGUI>();
            //_leaderPlaceImage = listItem.transform.Find("LeaderPlace/LeaderPlaceImage").GetComponent<Image>();
        }
        
        public void UpdateItem()
        {
            _playerNameText.text = PlayerName;
            _evoluteCount.text = EvoluteCount.ToString();
        }

        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }

        public void SetLeaderPlace(int place)
        {
            
        }
}
