using System;
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
    [SerializeField] private uint _leaderPlaceToShow = 3;
    
    public GameObject PanelGameObject;
    public GameObject ListItemPrefab;
    [SerializeField] private Sprite[] _leadersImages;
    [SerializeField] private GameObject _placeHolder;
    [SerializeField] private Transform ListItemParent;
    [SerializeField] private Image[] _leaderPlaceImages;
    


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
        
        int leaderBoardPlace = 1;
        for (int i = 0; i < _playerToShow; i++)
        {
            if (i >= players.Count)
                break;
            string playerName = CairoFieldsConverter.GetStringFromFieldElement(players[i].username);
            // start with 0x
            if(String.IsNullOrEmpty(playerName) || playerName.StartsWith("0x"))
                continue;
            
            LeaderboardItem leaderboardItem = CreateLeaderboardItem();
            leaderboardItem.PlayerName = CairoFieldsConverter.GetStringFromFieldElement(players[i].username);
            leaderboardItem.Address = players[i].player_id.Hex();
            leaderboardItem.EvoluteCount = players[i].balance;
            if (i < _leaderPlaceToShow)
            {
                leaderboardItem.SetLeaderPlace(leaderBoardPlace);
                leaderBoardPlace++;
            }
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
        public string Address;

        private TextMeshProUGUI _playerNameText;
        private TextMeshProUGUI _evoluteCount;
        private TextMeshProUGUI _addressText;
        private TextMeshProUGUI _placeText;
        private Image _leaderPlaceImage;
        private Button _copyButton;

        public LeaderboardItem(GameObject listItem)
        {
            ListItem = listItem;
            LeaderboardObjects leaderboardObjects = listItem.GetComponent<LeaderboardObjects>();
            _playerNameText = leaderboardObjects.PlayerName;
            _evoluteCount = leaderboardObjects.EvoluteCount;
            _leaderPlaceImage = leaderboardObjects.LeaderPlaceImage;
            _addressText = leaderboardObjects.Address;
            _placeText = leaderboardObjects.PlaceText;
            _copyButton = leaderboardObjects.CopyButton;
            _copyButton.onClick.AddListener(CopyAddress);
        }
        
        public void UpdateItem()
        {
            _playerNameText.text = PlayerName;
            _evoluteCount.text = "x " + EvoluteCount.ToString();
            _addressText.text = Address;
        }

        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }

        public void SetLeaderPlace(int place)
        {
            _placeText.text = place.ToString();
            _leaderPlaceImage.sprite = ListItem.GetComponent<LeaderboardObjects>().LeadersImages[place - 1];
            ListItem.GetComponent<LeaderboardObjects>().LeaderPlaceGameObject.SetActive(true);
        }
        
        public void CopyAddress()
        {
            GUIUtility.systemCopyBuffer = Address;
        }
}
