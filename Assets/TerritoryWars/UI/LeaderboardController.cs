using System.Collections.Generic;
using Dojo;
using TerritoryWars.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardController : MonoBehaviour
{
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

    public async void SetActivePanel(bool isActive)
    {
        if (isActive && MenuUIController.Instance._namePanelController.IsDefaultName())
        {
            MenuUIController.Instance._changeNamePanelUIController.SetNamePanelActive(true);
            return;
        }
        
        PanelGameObject.SetActive(isActive);
        
        
    }

    private void ClearAllListItems()
    {
        foreach (var matchListItem in _leaderboardItems)
        {
            Destroy(matchListItem.ListItem);
        }

        _leaderboardItems.Clear();
    }

    public void FetchData()
    {
        
    }

    public void ModelUpdated(ModelInstance modelInstance)
    {
        
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
            _leaderPlaceImage = listItem.transform.Find("LeaderPlace/LeaderPlaceImage").GetComponent<Image>();
        }

        public void SetActive(bool isActive)
        {
            ListItem.SetActive(isActive);
        }

        public void SetLeaderPlace(int place)
        {
            
        }
}
